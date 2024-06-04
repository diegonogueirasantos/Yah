using Newtonsoft.Json;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.BatchItemService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Common.Services.CachingService;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using System.Collections.Generic;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Identity;
using Amazon.DynamoDBv2.Model;
using Identity = Yah.Hub.Common.Identity.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Principal;
using Microsoft.AspNetCore.Server.IIS.Core;
using Nest;
using Amazon.Runtime.Internal;
using Yah.Hub.Domain.Monitor.Query;
using CsvHelper.Configuration;


namespace Yah.Hub.Marketplace.Application.Catalog
{
    public abstract class AbstractBatchCatalogService : AbstractCatalogService, IBatchCatalogService
    {
        IBatchItemService BatchItemService { get; }
        IElasticClient ElasticClient;
        public AbstractBatchCatalogService(
            IConfiguration configuration,
            ILogger<AbstractCatalogService> logger,
            IAccountConfigurationService configurationService,
            IIntegrationMonitorService integrationMonitorService,
            IBrokerService brokerService,
            IMarketplaceManifestService marketplaceManifestService,
            IValidationService validationService,
            ISecurityService securityService,
            ICacheService cacheService,
            IBatchItemService batchItemService,
            IElasticClient elasticClient) 
            : base(configuration, logger, configurationService, integrationMonitorService, brokerService, marketplaceManifestService, validationService, securityService, cacheService)
        {
            BatchItemService = batchItemService;
            ElasticClient = elasticClient;

            if (Handlers == null)
                Handlers = new Dictionary<Type, Func<dynamic, Task<ServiceMessage>>>();

            // announcement
            this.Handlers.TryAdd(typeof(RequestProductBatchState), (dynamic payload) => this.GetProductBatchStateAsync(payload));
            // announcement price
            this.Handlers.TryAdd(typeof(RequestPriceBatchState), (dynamic payload) => this.GetPriceBatchStateAsync(payload));
            // announcement inventory
            this.Handlers.TryAdd(typeof(RequestInventoryBatchState), (dynamic payload) => this.GetInventoryBatchStateAsync(payload));
        }


        #region [Abstract]
        public abstract Task<ServiceMessage<List<ProductIntegrationInfo>>> GetMarketplaceProductBatchStateAsync(MarketplaceServiceMessage<List<MarketplaceEntityState>> message);
        public abstract Task<ServiceMessage<List<PriceIntegrationInfo>>> GetMarketplacePriceBatchStateAsync(MarketplaceServiceMessage<List<MarketplaceEntityState>> message);
        public abstract Task<ServiceMessage<List<InventoryIntegrationInfo>>> GetMrketplaceInventoryBatchStateAsync(MarketplaceServiceMessage<List<MarketplaceEntityState>> message);
        public abstract Task<ServiceMessage<ProductIntegrationInfo[]>> InsertProductBatchAsync(MarketplaceServiceMessage<Product[]> message);
        public abstract Task<ServiceMessage<ProductIntegrationInfo[]>> UpdateProductBatchAsync(MarketplaceServiceMessage<Product[]> message);
        public abstract Task<ServiceMessage<ProductIntegrationInfo[]>> DeleteProductBatchAsync(MarketplaceServiceMessage<Product[]> message);
        public abstract Task<ServiceMessage<PriceIntegrationInfo[]>> UpdatePriceBatchAsync(MarketplaceServiceMessage<ProductPrice[]> message);
        public abstract Task<ServiceMessage<InventoryIntegrationInfo[]>> UpdateInventoryBatchAsync(MarketplaceServiceMessage<ProductInventory[]> message);
        public abstract Task<ServiceMessage<ProductIntegrationInfo[]>> UpdateImageBatchAsync(MarketplaceServiceMessage<List<MarketplaceEntityState>> message);
        protected virtual int GetMaxBatchSize() => 100;
        protected virtual TimeSpan ScrollTime() => TimeSpan.FromHours(1);
        #endregion

        public async Task<ServiceMessage> GetProductBatchStateAsync(ServiceMessage<CommandMessage<RequestProductBatchState>> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            if (string.IsNullOrEmpty(message.Data.Data.BatchId))
            {
                result.WithError(new Error("BatchId não informado", "", ErrorType.Business));
                return result;
            }

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(message.Identity, (GetCacheKey(message.Data.Data.GetType().Name, message.Data.Data.BatchId), message.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    var configuration = await this.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(message.Identity, GetMarketplace()));

                    var productsInfo = await this.GetMarketplaceEntities(new ServiceMessage<(string batchId, BatchType batchType)>(message.Identity, (message.Data.Data.BatchId, BatchType.PRODUCT)));

                    var integrationInfoResponse = await this.GetMarketplaceProductBatchStateAsync(productsInfo.Data.AsMarketplaceServiceMessage(message.Identity, configuration.Data));

                    if (!integrationInfoResponse.IsValid)
                    {
                        result.WithErrors(integrationInfoResponse.Errors);
                    }

                    if(integrationInfoResponse.Data != null && integrationInfoResponse.Data.Any())
                    {
                        integrationInfoResponse.Data.ForEach(x =>
                        {
                            var handleResult = base.HandleMarketplaceProductState(message.Identity, configuration.Data, message.Data, x).GetAwaiter().GetResult();

                            if (!handleResult.IsValid)
                            {
                                result.WithErrors(handleResult.Errors);
                            }
                        });
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));

                return result;
            }
            #endregion
        }

        public async Task<ServiceMessage> GetPriceBatchStateAsync(ServiceMessage<CommandMessage<RequestPriceBatchState>> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            if (string.IsNullOrEmpty(message.Data.Data.BatchId))
            {
                result.WithError(new Error("BatchId não informado", "", ErrorType.Business));
                return result;
            }

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(message.Identity, (GetCacheKey(message.Data.Data.GetType().Name, message.Data.Data.BatchId), message.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    var configuration = await this.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(message.Identity, GetMarketplace()));

                    var productsInfo = await this.GetMarketplaceEntities(new ServiceMessage<(string batchId, BatchType batchType)>(message.Identity, (message.Data.Data.BatchId, BatchType.PRICE)));

                    var integrationInfoResponse = await this.GetMarketplacePriceBatchStateAsync(productsInfo.Data.AsMarketplaceServiceMessage(message.Identity, configuration.Data));

                    if (!integrationInfoResponse.IsValid)
                    {
                        result.WithErrors(integrationInfoResponse.Errors);
                    }

                    if (integrationInfoResponse.Data != null && integrationInfoResponse.Data.Any())
                    {
                        integrationInfoResponse.Data.ForEach(x =>
                        {
                            var handleResult = base.HandleMarketplacePriceState(message.Identity, configuration.Data, message.Data, x).GetAwaiter().GetResult();

                            if (!handleResult.IsValid)
                            {
                                result.WithErrors(handleResult.Errors);
                            }
                        });
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));

                return result;
            }
            #endregion
        }

        public async Task<ServiceMessage> GetInventoryBatchStateAsync(ServiceMessage<CommandMessage<RequestInventoryBatchState>> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            if (string.IsNullOrEmpty(message.Data.Data.BatchId))
            {
                result.WithError(new Error("BatchId não informado", "", ErrorType.Business));
                return result;
            }

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(message.Identity, (GetCacheKey(message.Data.Data.GetType().Name, message.Data.Data.BatchId), message.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    var configuration = await this.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(message.Identity, GetMarketplace()));

                    var productsInfo = await this.GetMarketplaceEntities(new ServiceMessage<(string batchId, BatchType batchType)>(message.Identity,(message.Data.Data.BatchId, BatchType.INVENTORY)));

                    var integrationInfoResponse = await this.GetMrketplaceInventoryBatchStateAsync(productsInfo.Data.AsMarketplaceServiceMessage(message.Identity, configuration.Data));

                    if (!integrationInfoResponse.IsValid)
                    {
                        result.WithErrors(integrationInfoResponse.Errors);
                    }

                    if (integrationInfoResponse.Data != null && integrationInfoResponse.Data.Any())
                    {
                        integrationInfoResponse.Data.ForEach(x =>
                        {
                            var handleResult = base.HandleMarketplaceInventoryState(message.Identity, configuration.Data, message.Data, x).GetAwaiter().GetResult();

                            if (!handleResult.IsValid)
                            {
                                result.WithErrors(handleResult.Errors);
                            }
                        });
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));

                return result;
            }
            #endregion
        }
        public async override Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]
            var product = serviceMessage.Data.Data;

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity, new ProductIntegrationInfo(product.Id, product.Id, EntityStatus.Unknown,serviceMessage.Data.EventDateTime));

            var batchResult = await this.BatchItemService.SaveBatchItem(new ServiceMessage<BatchItem>(serviceMessage.Identity, new BatchItem()
            {
                EntityId = product.Id,
                Status = BatchStatus.WAITING,
                Data = JsonConvert.SerializeObject(product),
                Timestamp = DateTimeOffset.Now,
                Type = BatchType.PRODUCT,
                CommandType = Operation.Insert,
                Marketplace = GetMarketplace()
            }));

            if (!batchResult.IsValid)
            {
                if (batchResult.Errors.Any())
                {
                    result.WithErrors(batchResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar persistir o batch do Produto id {serviceMessage.Data.Data.Id}",$"erro ao persistir o batch do Produto id {serviceMessage.Data.Data.Id} no DynamoDB", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]
            var product = serviceMessage.Data.Data;

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity, new ProductIntegrationInfo(product.Id, product.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime));

            var batchResult = await this.BatchItemService.SaveBatchItem(new ServiceMessage<BatchItem>(serviceMessage.Identity, new BatchItem()
            {
                EntityId = product.Id,
                Status = BatchStatus.WAITING,
                Data = JsonConvert.SerializeObject(product),
                Timestamp = DateTimeOffset.Now,
                Type = BatchType.PRODUCT,
                CommandType = Operation.Update,
                Marketplace = GetMarketplace()
            }));

            if (!batchResult.IsValid)
            {
                if (batchResult.Errors.Any())
                {
                    result.WithErrors(batchResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar persistir o batch do Produto id {serviceMessage.Data.Data.Id}", $"erro ao persistir o batch do Produto id {serviceMessage.Data.Data.Id} no DynamoDB", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]
            var product = serviceMessage.Data.Data;

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity, new ProductIntegrationInfo(product.Id, product.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime));

            var batchResult = await this.BatchItemService.SaveBatchItem(new ServiceMessage<BatchItem>(serviceMessage.Identity, new BatchItem()
            {
                EntityId = product.Id,
                Status = BatchStatus.WAITING,
                Data = JsonConvert.SerializeObject(product),
                Timestamp = DateTimeOffset.Now,
                Type = BatchType.PRODUCT,
                CommandType = Operation.Delete,
                Marketplace = GetMarketplace()
            }));

            if (!batchResult.IsValid)
            {
                if (batchResult.Errors.Any())
                {
                    result.WithErrors(batchResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar persistir o batch do Produto id {serviceMessage.Data.Data.Id}", $"erro ao persistir o batch do Produto id {serviceMessage.Data.Data.Id} no DynamoDB", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            #region [Code]
            var productInventory = serviceMessage.Data.Data;

            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity, new InventoryIntegrationInfo()
            {
                Id = serviceMessage.Data.Data.AffectedSku.Id,
                ReferenceId = serviceMessage.Data.Data.Id,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            });

            var batchResult = await this.BatchItemService.SaveBatchItem(new ServiceMessage<BatchItem>(serviceMessage.Identity, new BatchItem()
            {
                EntityId = productInventory.AffectedSku.Id,
                Status = BatchStatus.WAITING,
                Data = JsonConvert.SerializeObject(productInventory),
                Timestamp = DateTimeOffset.Now,
                Type = BatchType.INVENTORY,
                CommandType = Operation.Update,
                Marketplace = GetMarketplace()
            }));

            if (!batchResult.IsValid)
            {
                if (batchResult.Errors.Any())
                {
                    result.WithErrors(batchResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar persistir o batch do Produto id {serviceMessage.Data.Data.Id}", $"erro ao persistir o batch do Produto id {serviceMessage.Data.Data.Id} no DynamoDB", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            #region [Code]
            var productInventory = serviceMessage.Data.Data;

            var result = new ServiceMessage<PriceIntegrationInfo>(serviceMessage.Identity, new PriceIntegrationInfo()
            {
                Id = serviceMessage.Data.Data.AffectedSku.Id,
                ReferenceId = serviceMessage.Data.Data.Id,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            });

            var batchResult = await this.BatchItemService.SaveBatchItem(new ServiceMessage<BatchItem>(serviceMessage.Identity, new BatchItem()
            {
                EntityId = productInventory.AffectedSku.Id,
                Status = BatchStatus.WAITING,
                Data = JsonConvert.SerializeObject(productInventory),
                Timestamp = DateTimeOffset.Now,
                Type = BatchType.PRICE,
                CommandType = Operation.Update,
                Marketplace = GetMarketplace()
            }));

            if (!batchResult.IsValid)
            {
                if (batchResult.Errors.Any())
                {
                    result.WithErrors(batchResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar persistir o batch do Produto id {serviceMessage.Data.Data.Id}", $"erro ao persistir o batch do Produto id {serviceMessage.Data.Data.Id} no DynamoDB", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async Task<ServiceMessage> ProcessBatchAsync(ServiceMessage<(BatchType BatchType, Operation CommandType)> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage(serviceMessage.Identity);
            
            var config = await base.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, GetMarketplace()));
           
            var data = new BatchItemMessage() { BatchType = serviceMessage.Data.BatchType, Status = BatchStatus.WAITING, CommandType = serviceMessage.Data.CommandType, Marketplace = config.Data.Marketplace, MaxDoc = GetMaxBatchSize() };
            var query = new BatchItemQuery(BatchStatus.WAITING, this.GetMarketplace(), serviceMessage.Data.BatchType, serviceMessage.Data.CommandType, GetMaxBatchSize());

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(serviceMessage.Identity, (GetCacheKey(serviceMessage.Data.BatchType.ToString(), serviceMessage.Data.CommandType.ToString()), DateTimeOffset.Now, TimeSpan.FromMinutes(10)))))
                {

                    if (!scope.IsLocked)
                    {
                        return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error("Existe um processamento em andamento.","",ErrorType.Business));
                    }

                    var requestMessage = new ServiceMessage<BatchItemQuery>(serviceMessage.Identity, new BatchItemQuery(data.Status,data.Marketplace,data.BatchType,data.CommandType, GetMaxBatchSize()));

                    var batchItems = await this.BatchItemService.GetBachItems(requestMessage);

                    if (!batchItems.IsValid)
                    {
                        if (batchItems.Errors.Any())
                        {
                            result.WithErrors(batchItems.Errors);

                            return result;
                        }
                        else
                        {
                            return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error($"Não foi possível obter os lotes para o marketplace {config.Data.Marketplace}", $"Erro desconhecido ao tentar obter os lotes de batch", ErrorType.Technical));
                        } 
                    }

                    if (data.BatchType.Equals(BatchType.PRODUCT))
                    {
                        var resultProductBatch = await this.ProcessProductBatchAsync(batchItems.Data.AsMarketplaceServiceMessage(serviceMessage.Identity,config.Data), data.CommandType);

                        if (!resultProductBatch.IsValid)
                        {
                            if (resultProductBatch.Errors.Any())
                            {
                                result.WithErrors(batchItems.Errors);

                                return result;
                            }
                            else
                            {
                                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error($"Não foi possível enviar os lotes para o marketplace {config.Data.Marketplace}", $"Erro desconhecido ao tentar enviar os lotes de batch", ErrorType.Technical));
                            }
                        }

                        foreach(var productState in resultProductBatch.Data)
                        {   
                            var message = new CommandMessage<ProductIntegrationInfo>(serviceMessage.Identity)
                            {
                                Data = productState,
                                ServiceOperation = requestMessage.Data.CommandType,
                                EventDateTime = DateTimeOffset.Now,
                                CorrelationId = Guid.NewGuid().ToString(),
                                Marketplace = GetMarketplace()
                            };

                            await this.BrokerService.EnqueueCommandAsync(message);
                        }
                    }
                    else if (data.BatchType.Equals(BatchType.PRICE))
                    {
                        var resultPriceBatch = await this.ProcessPriceBatchAsync(batchItems.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, config.Data));

                        if (!resultPriceBatch.IsValid)
                        {
                            if (resultPriceBatch.Errors.Any())
                            {
                                result.WithErrors(batchItems.Errors);

                                return result;
                            }
                            else
                            {
                                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error($"Não foi possível enviar os lotes para o marketplace {config.Data.Marketplace}", $"Erro desconhecido ao tentar enviar os lotes de batch", ErrorType.Technical));
                            }
                        }

                        foreach (var priceState in resultPriceBatch.Data)
                        {
                            var message = new CommandMessage<PriceIntegrationInfo>(serviceMessage.Identity)
                            {
                                Data = priceState,
                                ServiceOperation = requestMessage.Data.CommandType,
                                EventDateTime = DateTimeOffset.Now,
                                CorrelationId = Guid.NewGuid().ToString(),
                                Marketplace = GetMarketplace()
                            };

                            await this.BrokerService.EnqueueCommandAsync(message);
                        }
                    }
                    else if (data.BatchType.Equals(BatchType.INVENTORY))
                    {
                        var resultInventoryBatch = await this.ProcessInventoryBatchAsync(batchItems.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, config.Data));

                        if (!resultInventoryBatch.IsValid)
                        {
                            if (resultInventoryBatch.Errors.Any())
                            {
                                result.WithErrors(batchItems.Errors);

                                return result;
                            }
                            else
                            {
                                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error($"Não foi possível enviar os lotes para o marketplace {config.Data.Marketplace}", $"Erro desconhecido ao tentar enviar os lotes de batch", ErrorType.Technical));
                            }
                        }

                        foreach (var inventoryState in resultInventoryBatch.Data)
                        {
                            var message = new CommandMessage<InventoryIntegrationInfo>(serviceMessage.Identity)
                            {
                                Data = inventoryState,
                                ServiceOperation = requestMessage.Data.CommandType,
                                EventDateTime = DateTimeOffset.Now,
                                CorrelationId = Guid.NewGuid().ToString(),
                                Marketplace = GetMarketplace()
                            };

                            await this.BrokerService.EnqueueCommandAsync(message);
                        }
                    }

                    if (result.IsValid)
                    {
                        await scope.PersistTimestampKeyAsync(new ServiceMessage<LockState>(serviceMessage.Identity, scope));
                    }
                }

            }
            catch(Exception ex)
            {
                result.WithError(new Error(ex));

                return result;
            }


            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> GetProductIntegrationStatus(MarketplaceServiceMessage<RequestProductState> message)
        {
            throw new NotImplementedException();
        }
        #region [Private]

        private async Task<ServiceMessage<List<ProductIntegrationInfo>>> ProcessProductBatchAsync(MarketplaceServiceMessage<List<BatchItem>> message, Operation operation)
        {
            #region [Code]
            var result = new ServiceMessage<List<ProductIntegrationInfo>>(message.Identity);

            var productResultList = new List<ProductIntegrationInfo>();

            if (operation.Equals(Operation.Insert))
            {
                var resultRequest = await this.InsertProductBatchAsync(message.Data.Select(x => x.Product).ToArray().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if (!resultRequest.IsValid)
                {
                    if (resultRequest.Errors.Any())
                    {
                        result.WithErrors(resultRequest.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"Erro desconhecido ao enviar o lote para o marketplace {GetMarketplace()}", $"Erro desconhecido ao enviar o lote para o marketplace", ErrorType.Technical));
                    }

                    return result;
                }

                result.WithData(resultRequest.Data.ToList());
            }
            else if (operation.Equals(Operation.Update))
            {
                var resultRequest = await this.UpdateProductBatchAsync(message.Data.Select(x => x.Product).ToArray().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if (!resultRequest.IsValid)
                {
                    if (resultRequest.Errors.Any())
                    {
                        result.WithErrors(resultRequest.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"Erro desconhecido ao enviar o lote para o marketplace {GetMarketplace()}", $"Erro desconhecido ao enviar o lote para o marketplace", ErrorType.Technical));
                    }

                    return result;
                }

                result.WithData(resultRequest.Data.ToList());
            }
            else if (operation.Equals(Operation.Delete))
            {
                var resultRequest = await this.DeleteProductBatchAsync(message.Data.Select(x => x.Product).ToArray().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if (!resultRequest.IsValid)
                {
                    if (resultRequest.Errors.Any())
                    {
                        result.WithErrors(resultRequest.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"Erro desconhecido ao enviar o lote para o marketplace {GetMarketplace()}", $"Erro desconhecido ao enviar o lote para o marketplace", ErrorType.Technical));
                    }

                    return result;
                }

                result.WithData(resultRequest.Data.ToList());
            }

            return result;
            #endregion
        }

        private async Task<ServiceMessage<List<PriceIntegrationInfo>>> ProcessPriceBatchAsync(MarketplaceServiceMessage<List<BatchItem>> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<PriceIntegrationInfo>>(message.Identity);

            var productResultList = new List<PriceIntegrationInfo>();

            var resultRequest = await this.UpdatePriceBatchAsync(message.Data.Select(x => x.Price).ToArray().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!resultRequest.IsValid)
            {
                if (resultRequest.Errors.Any())
                {
                    result.WithErrors(resultRequest.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro desconhecido ao enviar o lote para o marketplace {GetMarketplace()}", $"Erro desconhecido ao enviar o lote para o marketplace", ErrorType.Technical));
                }

                return result;
            }

            result.WithData(resultRequest.Data.ToList());


            return result;
            #endregion
        }

        private async Task<ServiceMessage<List<InventoryIntegrationInfo>>> ProcessInventoryBatchAsync(MarketplaceServiceMessage<List<BatchItem>> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<InventoryIntegrationInfo>>(message.Identity);

            var productResultList = new List<InventoryIntegrationInfo>();

            var resultRequest = await this.UpdateInventoryBatchAsync(message.Data.Select(x => x.Inventory).ToArray().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!resultRequest.IsValid)
            {
                if (resultRequest.Errors.Any())
                {
                    result.WithErrors(resultRequest.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro desconhecido ao enviar o lote para o marketplace {GetMarketplace()}", $"Erro desconhecido ao enviar o lote para o marketplace", ErrorType.Technical));
                }

                return result;
            }

            result.WithData(resultRequest.Data.ToList());


            return result;
            #endregion
        }

        private async Task<ServiceMessage<List<MarketplaceEntityState>>> GetMarketplaceEntities(ServiceMessage<(string batchId, BatchType batchType)> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<MarketplaceEntityState>>(message.Identity);

            var productsInfo = new List<MarketplaceEntityState>();
            
            var response = await this.IntegrationMonitorService.GetEntityStateByBatchId(new BatchQueryRequest(GetMaxBatchSize(), ScrollTime(), message.Data.batchType, message.Data.batchId).AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));
            
            if (!response.IsValid)
            {
                return ServiceMessage<List<MarketplaceEntityState>>.CreateInvalidResult(message.Identity, new Error($"Não foi possível obter os produtos para consulta de status no marketplace {GetMarketplace().ToString()}", response.DebugInformation, ErrorType.Technical), null);
            }

            if (!response.Documents.Any())
                return ServiceMessage<List<MarketplaceEntityState>>.CreateValidResult(message.Identity);

            try
            {
                do
                {
                    productsInfo.AddRange(response.Documents);

                    response = await ElasticClient.ScrollAsync<MarketplaceEntityState>(ScrollTime(), response.ScrollId);

                } while (response.IsValid && response.Documents.Any());

                if (!string.IsNullOrWhiteSpace(response.ScrollId))
                    await ElasticClient.ClearScrollAsync(x => x.ScrollId(response.ScrollId));

                result.WithData(productsInfo);

                return result;
            }
            catch (Exception ex)
            {
                var clearScrollResult = await ElasticClient.ClearScrollAsync(x => x.ScrollId(response.ScrollId));

                if (!clearScrollResult.IsValid)
                    Logger.LogCustomCritical(new Error($"Error while clear ScrollId {response.ScrollId} on ElasticSearch.", clearScrollResult.DebugInformation, ErrorType.Technical), message.Identity);

                result.WithError(new Error(ex));

                return result;
            }
            #endregion
        }

        #endregion
    }
}
