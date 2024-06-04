using System;
using Nest;
using System.Text;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.IntegrationMonitorRepository;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Common.Services.CachingService;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using System.Runtime.Remoting;
using System.Reflection;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Security;
using Microsoft.CSharp.RuntimeBinder;
using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Domain.Monitor.Query;

namespace Yah.Hub.Application.Services.IntegrationMonitorService
{
    public partial class IntegrationMonitorService : AbstractService, IIntegrationMonitorService
    {
        protected IIntegrationMonitorRepository IntegrationMonitorRepository { get; set; }
        protected ICacheService CacheService { get; set; }
        protected IBrokerService Broker { get; set; }
        private readonly IElasticClient ElasticClient;
        private readonly ISecurityService SecurityService;

        private readonly IDictionary<EntityType, Func<MarketplaceServiceMessage<string>, Task<ServiceMessage>>> EnqueueHandler;

        public IntegrationMonitorService(
            IIntegrationMonitorRepository integrationMonitorRepository,
            IConfiguration configuration, ILogger<IntegrationMonitorService> logger,
            ICacheService cacheService,
            IElasticClient elasticClient,
            IBrokerService broker,
            ISecurityService securityService)
            : base(configuration, logger)
        {
            IntegrationMonitorRepository = integrationMonitorRepository;
            CacheService = cacheService;
            ElasticClient = elasticClient;
            Broker = broker;
            SecurityService = securityService;

            EnqueueHandler = new Dictionary<EntityType, Func<MarketplaceServiceMessage<string>, Task<ServiceMessage>>>
            {
                [EntityType.Product] = EnqueueProductBatchRequestInfo,
                [EntityType.Price] = EnqueuePriceBatchRequestInfo,
                [EntityType.Inventory] = EnqueueInventoryBatchRequestInfo
            };
        }

        public async Task<ServiceMessage<List<MarketplaceEntityState>>> GetBtRefferId(MarketplaceServiceMessage<string> serviceMessage)
        {
            return await this.IntegrationMonitorRepository.GetByReferenceId(serviceMessage);
        }

        public async Task<ServiceMessage<List<bool>>> HandleCommands()
        {
            #region [Code]
            var identity = await SecurityService.IssueWorkerIdentity();

            var result = new ServiceMessage<List<bool>>(identity, new List<bool>());

            var request = new CommandMessage<dynamic>(identity);
            var batches = this.Broker.PeekCommand<dynamic>(request);

            foreach (var batch in batches)
            {
                var dequeueBatch = new DequeueCommandBatchMessage<dynamic>();
                dequeueBatch.Commands = new List<DequeueCommandMessage>();

                foreach(var command in batch)
                {
                    try
                    {
                        string typeString = (string)command.Command.AssemblyQualifiedName;
                        MarketplaceAlias marketplaceEnum = command.Command.Marketplace;

                        var currentType = Type.GetType(typeString) ?? null;
                        var typedCommand = Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(command.Command), currentType);

                        try
                        {
                            bool dequeue = false;

                            if (typedCommand.Data != null)
                            {
                                ServiceMessage handleResult = await this.HandleEntityState(typedCommand);
                                dequeue = handleResult.IsValid;
                            }
                            else
                                dequeue = true;

                            if (dequeue)
                            {
                                dequeueBatch.Commands.Add(new DequeueCommandMessage() { Marketplace = marketplaceEnum.ToString(), MessageId = command.MessageId, ReceiptHandle = command.ReceiptHandle });
                                result.Data.Add(true);
                            }
                          
                        }
                        catch (RuntimeBinderException ex)
                        {
                            result.WithError(new Error("Unrecognized Type", $"Command: {Newtonsoft.Json.JsonConvert.SerializeObject(command)}", ErrorType.Technical));
                            dequeueBatch.Commands.Add(new DequeueCommandMessage() { Marketplace = marketplaceEnum.ToString(), MessageId = command.MessageId, ReceiptHandle = command.ReceiptHandle });
                            result.Data.Add(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Error error = new Error(ex);
                        Logger.LogCustomCritical(error, identity);
                        result.WithError(new Error(ex));
                        result.Data.Add(false);
                    }
                };

                if (dequeueBatch.Commands.Any())
                {
                    var dequeueResult = this.Broker.DequeueCommandBatchAsync<dynamic>(dequeueBatch.AsServiceMessage(identity)).GetAwaiter().GetResult();
                    result.WithErrors(dequeueResult.Errors);
                }
            }

            return result;
            #endregion
        }

        // code below depends on NH-44 thats allow use predicates on any elastic repositories
        //public ServiceMessage<ProductInfo> GetProductById(ServiceMessage<string> serviceMessage)
        //{
        //}

        //public ServiceMessage<ProductInfo> GetProductsByIntegrationStatus(ServiceMessage<EntityStatus> serviceMessage)
        //{
        //}

        //public ServiceMessage<SkuInfo> GetSkuById(ServiceMessage serviceMessage)
        //{
        //}

        //public ServiceMessage<AnnouncementInfo> GetAnnouncementById(ServiceMessage serviceMessage)
        //{
        //}

        public async Task<ServiceMessage<EntityStateSearchResult>> QueryAsync(MarketplaceServiceMessage<EntityStateQuery> message)
        {
            return await this.IntegrationMonitorRepository.QueryAsync(message);
        }

        public async Task<ServiceMessage<List<IntegrationSummary>>> GetIntegrationSummary(MarketplaceServiceMessage serviceMessage)
        {
            return await this.IntegrationMonitorRepository.GetIntegrationSummary(serviceMessage);
        }

        #region [Monitor]
        public async Task<ServiceMessage> MonitorIntegrationStatus(MarketplaceServiceMessage<(EntityType type, bool HasBatchSystem)> serviceMessage)
        {
            #region [Code]

            using (var scope = new LockState(
                this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>
                (serviceMessage.Identity, (GetCacheKey(serviceMessage.Data.type), DateTimeOffset.Now, TimeSpan.FromMinutes(30)))))
            {
                if (!scope.IsLocked)
                {
                    return ServiceMessage.CreateValidResult(serviceMessage.Identity);
                }

                var request = new MonitorStatusRequest(100, TimeSpan.FromHours(1), 30).AsMarketplaceServiceMessage(serviceMessage);

                var response = await this.IntegrationMonitorRepository.GetEntitiesByStatusRequest(request, serviceMessage.Data.type);

                var result = new ServiceMessage(serviceMessage.Identity);

                if (!response.IsValid)
                {
                    return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error($"Não foi possível obter os produtos para consulta de status no marketplace {serviceMessage.Marketplace.ToString()}", response.DebugInformation, ErrorType.Technical));
                }

                if (!response.Documents.Any())
                    return ServiceMessage.CreateValidResult(serviceMessage.Identity);

                #region [Non-BatchSystem]
                if (!serviceMessage.Data.HasBatchSystem)
                {
                    do
                    {
                        foreach (var item in response.Documents)
                        {
                            try
                            {
                                result = await EnqueueProductRequestInfo(item.AsMarketplaceServiceMessage(serviceMessage));
                            }
                            catch (Exception ex)
                            {
                                var clearScrollResult = await ElasticClient.ClearScrollAsync(x => x.ScrollId(response.ScrollId));

                                if (!clearScrollResult.IsValid)
                                    Logger.LogCustomCritical(new Error($"Error while clear ScrollId {response.ScrollId} on ElasticSearch.", clearScrollResult.DebugInformation, ErrorType.Technical), serviceMessage.Identity);

                            }
                        }

                        response = await ElasticClient.ScrollAsync<MarketplaceEntityState>(request.Data.ScrollTime, response.ScrollId);

                    } while (response.IsValid && response.Documents.Any());

                    if (!string.IsNullOrWhiteSpace(response.ScrollId))
                        await ElasticClient.ClearScrollAsync(x => x.ScrollId(response.ScrollId));

                    return result;
                }
                #endregion

                #region [BatchSystem]

                EnqueueHandler.TryGetValue(serviceMessage.Data.type, out var handler);
                
                var batches = response.Aggregations.Terms("batch").Buckets
                                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                                .ToArray();

                foreach (var batchId in batches)
                {
                    result.Merge(await handler(batchId.Key.AsMarketplaceServiceMessage(serviceMessage)));
                }

                return result;
                #endregion

            }

            #endregion
        }
        #endregion

        #region [Enqueue]
        private async Task<ServiceMessage> EnqueueProductRequestInfo(MarketplaceServiceMessage<MarketplaceEntityState> productState)
        {
            var identity = productState.Identity;
            var message = new CommandMessage<RequestProductState>(identity)
            {
                Data = new RequestProductState(productState.Data.ProductInfo.Id, productState.Data.ProductInfo.ReferenceId, productState.Data.ProductInfo.IntegrationId,productState.Data.ProductInfo.Skus),
                CorrelationId = Guid.NewGuid().ToString(),
                IsSync = false,
                EventDateTime = DateTime.Now,
                Marketplace = productState.Marketplace
            };

            return await this.Broker.EnqueueCommandAsync(message);
        }

        private async Task<ServiceMessage> EnqueueProductBatchRequestInfo(MarketplaceServiceMessage<string> productState)
        {
            var identity = productState.Identity;
            var message = new CommandMessage<RequestProductBatchState>(identity)
            {
                Data = new RequestProductBatchState(productState.Data),
                CorrelationId = Guid.NewGuid().ToString(),
                IsSync = false,
                EventDateTime = DateTime.Now,
                Marketplace = productState.Marketplace
            };

            return await this.Broker.EnqueueCommandAsync(message);
        }

        private async Task<ServiceMessage> EnqueuePriceBatchRequestInfo(MarketplaceServiceMessage<string> productState)
        {
            var identity = productState.Identity;
            var message = new CommandMessage<RequestPriceBatchState>(identity)
            {
                Data = new RequestPriceBatchState(productState.Data),
                CorrelationId = Guid.NewGuid().ToString(),
                IsSync = false,
                EventDateTime = DateTime.Now,
                Marketplace = productState.Marketplace
            };

            return await this.Broker.EnqueueCommandAsync(message);
        }

        private async Task<ServiceMessage> EnqueueInventoryBatchRequestInfo(MarketplaceServiceMessage<string> productState)
        {
            var identity = productState.Identity;
            var message = new CommandMessage<RequestInventoryBatchState>(identity)
            {
                Data = new RequestInventoryBatchState(productState.Data),
                CorrelationId = Guid.NewGuid().ToString(),
                IsSync = false,
                EventDateTime = DateTime.Now,
                Marketplace = productState.Marketplace
            };

            return await this.Broker.EnqueueCommandAsync(message);
        }
        #endregion

        #region [Private]
        private static string GetCacheKey(EntityType type)
        {
            return $"{type.ToString()}IntergationInfo:monitor";
        }

        #endregion

        public async Task<ISearchResponse<MarketplaceEntityState>> GetEntityStateByBatchId(MarketplaceServiceMessage<BatchQueryRequest> message)
        {
            return await this.IntegrationMonitorRepository.GetEntitiesByBatchId(message);
        }
    }
}

