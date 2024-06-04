using System;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Common.Identity;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Data.Repositories.AccountConfigurationRepository;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Amazon.DynamoDBv2.Model;
using System.Security.Principal;
using Identity = Yah.Hub.Common.Identity.Identity;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Domain.Manifest;
using Yah.Hub.Application.Broker.Messages;
using Amazon.Runtime.Internal.Util;
using Nest;
using Yah.Hub.Common.Security; 
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Common.Services.CachingService;

namespace Yah.Hub.Marketplace.Application.Catalog
{
    public abstract class AbstractCatalogService : AbstractMarketplaceService, ICatalogService
    {
        #region [Constant]
        private readonly string Product = "product";
        private readonly string ProductPrice = "productprice";
        private readonly string ProductInventory = "productinventory";
        private readonly string ProductIntegration = "productintegration";
        #endregion

        #region Properties

        protected IAccountConfigurationService ConfigurationService { get; }
        protected IIntegrationMonitorService IntegrationMonitorService { get; set; }
        protected IBrokerService BrokerService { get; set; }
        protected IMarketplaceManifestService MarketplaceManifestService { get; set; }
        protected IValidationService ValidationService { get; }
        protected ICacheService CacheService { get; set; }
        private readonly ISecurityService SecurityService;


        protected Dictionary<Type, Func<dynamic, Task<ServiceMessage>>> Handlers { get; set; }

        #endregion

        #region Constructor

        public AbstractCatalogService(
            IConfiguration configuration, 
            ILogger<AbstractCatalogService> logger, 
            IAccountConfigurationService configurationService, 
            IIntegrationMonitorService integrationMonitorService,
            IBrokerService brokerService,
            IMarketplaceManifestService marketplaceManifestService,
            IValidationService validationService,
            ISecurityService securityService,
            ICacheService cacheService) 
            : base(configuration, logger)
        {
            ConfigurationService = configurationService;
            IntegrationMonitorService = integrationMonitorService;
            BrokerService = brokerService;
            MarketplaceManifestService = marketplaceManifestService;
            ValidationService = validationService;
            SecurityService = securityService;
            CacheService = cacheService;

            if (Handlers == null)
                Handlers = new Dictionary<Type, Func<dynamic, Task<ServiceMessage>>>();

            // product
            Handlers.TryAdd(typeof(Product), (dynamic payload) => this.ExecuteProductCommand(payload));
            // price
            Handlers.TryAdd(typeof(ProductPrice), (dynamic payload) => this.ExecuteProductPriceCommand(payload));
            // inventory
            Handlers.TryAdd(typeof(ProductInventory), (dynamic payload) => this.ExecuteProductInventoryCommand(payload));
            //ProductState
            Handlers.TryAdd(typeof(RequestProductState), (dynamic payload) => this.ExecuteProductIntegrationStatusCommand(payload));
            
        }

        #endregion

        #region Abstract

        // product
        public abstract Task<ServiceMessage<ProductIntegrationInfo>> GetProductIntegrationStatus(MarketplaceServiceMessage<RequestProductState> message);
        public abstract Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage);
        public abstract Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage);
        public abstract Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage);

        //price
        public abstract Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage);

        //inventory
        public abstract Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage);

        #endregion

        #region [Virtual Methods]

        protected virtual async Task<ServiceMessage> DisableAccount(MarketplaceServiceMessage message)
        {
            return ServiceMessage.CreateValidResult(message.Identity);
        }

        #endregion

        #region Consumers

        public virtual async Task<ServiceMessage> ConsumeProductCommand(ServiceMessage serviceMessage)
        {
            return await this.ConsumeCommand(new ServiceMessage<Product>(serviceMessage.Identity));
        }

        public virtual async Task<ServiceMessage> ConsumeProductPriceCommand(ServiceMessage serviceMessage)
        {
            return await this.ConsumeCommand(new ServiceMessage<ProductPrice>(serviceMessage.Identity));
        }

        public virtual async Task<ServiceMessage> ConsumeProductInventoryCommand(ServiceMessage serviceMessage)
        { 
            return await this.ConsumeCommand(new ServiceMessage<ProductInventory>(serviceMessage.Identity));
        }

        public virtual async Task<ServiceMessage> ConsumeProductRequestState(ServiceMessage serviceMessage)
        {
            return await this.ConsumeCommand(new ServiceMessage<RequestProductState>(serviceMessage.Identity));
        }

        #endregion

        #region Producers

        public virtual async Task<ServiceMessage> EnqueueProductCommand(ServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            serviceMessage.Data.Identity = serviceMessage.Identity;
            return await this.BrokerService.EnqueueCommandAsync<Product>(serviceMessage.Data);
        }

        public virtual async Task<ServiceMessage> EnqueueProductPriceCommand(ServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            serviceMessage.Data.Identity = serviceMessage.Identity;
            return await this.BrokerService.EnqueueCommandAsync<ProductPrice>(serviceMessage.Data);
        }

        public virtual async Task<ServiceMessage> EnqueueProductInventoryCommand(ServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            serviceMessage.Data.Identity = serviceMessage.Identity;
            return await this.BrokerService.EnqueueCommandAsync<ProductInventory>(serviceMessage.Data);
        }

        #endregion

        #region Execute Commands

        public virtual async Task<ServiceMessage> ExecuteProductCommand(ServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]
            if (serviceMessage.Data == null)
            {
                var reason = new Error("Necessário informar um produto para realizar a integração!", "", ErrorType.Business) ;

                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, reason);
            }

            var identity = serviceMessage.Identity;
            var message = serviceMessage.Data;
            var product = message.Data;

            var result = ServiceMessage<ProductIntegrationInfo>.CreateValidResult(identity);

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(identity,(GetCacheKey(Product, product.Id),serviceMessage.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    if (!scope.IsLocked)
                    {
                        if (scope.IsObsolete)
                        {
                            return result;
                        }
                        else
                        {
                            result.WithError(new Error("Locked!", "Method or Operation is already in execution", ErrorType.Technical));
                            return result;

                        }
                    }

                    var configuration = await this.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, GetMarketplace()));

                    //var manifest = await this.MarketplaceManifestService.GetManifestAsync(new MarketplaceServiceMessage(configuration.Identity, configuration.Data.Marketplace));

                    if (message.ServiceOperation.In(Operation.Insert, Operation.Update))
                    {
                        //var validationResult = await this.ValidationService.Validate(new MarketplaceServiceMessage<(Product Product, MarketplaceManifest Manifest)>(identity, configuration.Data, (product, manifest)));

                        //if (validationResult.Data != null && validationResult.Data.Errors.Any())
                        //{
                        //    var handleResult = await this.HandleMarketplaceProductState(identity, configuration.Data, message, validationResult.Data);

                        //    if (!handleResult.IsValid)
                        //    {
                        //        result.WithErrors(handleResult.Errors);
                        //    }
                        //    return result;
                        //}

                        if (message.ServiceOperation == Operation.Insert)
                            result = await this.CreateProduct(message.AsMarketplaceServiceMessage(identity, configuration.Data));
                        if (message.ServiceOperation == Operation.Update)
                            result = await this.UpdateProduct(message.AsMarketplaceServiceMessage(identity, configuration.Data));
                    }

                    if (message.ServiceOperation == Operation.Delete)
                        result = await this.DeleteProduct(message.AsMarketplaceServiceMessage(identity, configuration.Data));


                    if (result.Data != null)
                    {
                        if (result.Data.IntegrationActionResult == IntegrationActionResult.Retry)
                        {
                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {message.Data.Id}", "Throttling", ErrorType.Business));
                        }
                        else if (result.Data.IntegrationActionResult == IntegrationActionResult.NotAuthorized)
                        {
                            await this.DisableAccount(message.AsMarketplaceServiceMessage(identity, configuration.Data));

                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {message.Data.Id}", "Não Autorizado", ErrorType.Business));
                        }

                        var handleResult = await this.HandleMarketplaceProductState(identity, configuration.Data, message, result.Data);

                        if (!handleResult.IsValid)
                        {
                            result.WithErrors(handleResult.Errors);
                        }
                    }

                    if (result.IsValid)
                    {
                        await scope.PersistTimestampKeyAsync(new ServiceMessage<LockState>(identity, scope));
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to execute product command");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public virtual async Task<ServiceMessage> ExecuteProductPriceCommand(ServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            #region [Code]

            if (serviceMessage.Data == null)
            {
                var reason = new Error("Necessário informar um produto para realizar a integração do preço!", "", ErrorType.Business);

                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, reason);
            }
            var message = serviceMessage.Data;
            var identity = serviceMessage.Identity;
            var affectedSku = serviceMessage.Data.Data.AffectedSku;

            var result = new ServiceMessage<PriceIntegrationInfo>(serviceMessage.Identity);

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(identity, (GetCacheKey(ProductPrice, affectedSku.Id), serviceMessage.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    if (!scope.IsLocked)
                    {
                        if (scope.IsObsolete)
                        {
                            return result;
                        }
                        else
                        {
                            result.WithError(new Error("Locked!", "Method or Operation is already in execution", ErrorType.Technical));
                            return result;

                        }
                    }

                    var configuration = await this.ConfigurationService.GetConfiguration(serviceMessage.AsMarketplaceServiceMessage(serviceMessage.Identity, GetMarketplace()));

                    if (message.ServiceOperation.In(Operation.Insert, Operation.Update))
                    {

                        if (message.ServiceOperation == Operation.Update)
                            result = await this.UpdatePrice(message.AsMarketplaceServiceMessage(serviceMessage.Identity, configuration.Data));
                    }

                    if (result.Data != null)
                    {
                        if (result.Data.IntegrationActionResult == IntegrationActionResult.Retry)
                        {
                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {message.Data.Id}", "Throttling", ErrorType.Business));
                        }
                        else if (result.Data.IntegrationActionResult == IntegrationActionResult.NotAuthorized)
                        {
                            await this.DisableAccount(message.AsMarketplaceServiceMessage(serviceMessage.Identity, configuration.Data));

                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {message.Data.Id}", "Não Autorizado", ErrorType.Business));
                        }

                        var handleResult = await this.HandleMarketplacePriceState(serviceMessage.Identity, configuration.Data, message, result.Data);

                        if (!handleResult.IsValid)
                        {
                            result.WithErrors(handleResult.Errors);
                        }
                    }

                    if (result.IsValid)
                    {
                        await scope.PersistTimestampKeyAsync(new ServiceMessage<LockState>(identity, scope));
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to execute price command");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> ExecuteProductInventoryCommand(ServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            #region [Code]

            if (serviceMessage.Data == null)
            {
                var reason = new Error("Necessário informar um produto para realizar a integração do estoque!", "", ErrorType.Business);

                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, reason);
            }

            var identity = serviceMessage.Identity;
            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity);
            var message = serviceMessage.Data;
            var affectedSku = serviceMessage.Data.Data.AffectedSku;

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(identity, (GetCacheKey(ProductInventory, affectedSku.Id), serviceMessage.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    if (!scope.IsLocked)
                    {
                        if (scope.IsObsolete)
                        {
                            return result;
                        }
                        else
                        {
                            result.WithError(new Error("Locked!", "Method or Operation is already in execution", ErrorType.Technical));
                            return result;

                        }
                    }

                    var configuration = await this.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, GetMarketplace()));

                    if (message.ServiceOperation.In(Operation.Insert, Operation.Update))
                    {

                        if (message.ServiceOperation == Operation.Update)
                            result = await this.UpdateInventory(message.AsMarketplaceServiceMessage(serviceMessage.Identity, configuration.Data));
                    }

                    if (result.Data != null)
                    {
                        if (result.Data.IntegrationActionResult == IntegrationActionResult.Retry)
                        {
                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {message.Data.Id}", "Throttling", ErrorType.Business));
                        }
                        else if (result.Data.IntegrationActionResult == IntegrationActionResult.NotAuthorized)
                        {
                            await this.DisableAccount(message.AsMarketplaceServiceMessage(serviceMessage.Identity, configuration.Data));

                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {message.Data.Id}", "Não Autorizado", ErrorType.Business));
                        }

                        var handleResult = await this.HandleMarketplaceInventoryState(serviceMessage.Identity, configuration.Data, message, result.Data);

                        if (!handleResult.IsValid)
                        {
                            result.WithErrors(handleResult.Errors);
                        }
                    }

                    if (result.IsValid)
                    {
                        await scope.PersistTimestampKeyAsync(new ServiceMessage<LockState>(identity, scope));
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to execute inventory command");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> ExecuteProductIntegrationStatusCommand(ServiceMessage<CommandMessage<RequestProductState>> serviceMessage)
        {
            #region [Code]
            var identity = serviceMessage.Identity;
            var result = new ServiceMessage(identity);
            var productState = serviceMessage.Data.Data;

            try
            {
                using (var scope = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(identity, (GetCacheKey(ProductInventory, productState.Id), serviceMessage.Data.EventDateTime, TimeSpan.FromMinutes(10)))))
                {
                    if (!scope.IsLocked)
                    {
                        if (scope.IsObsolete)
                        {
                            return result;
                        }
                        else
                        {
                            result.WithError(new Error("Locked!", "Method or Operation is already in execution", ErrorType.Technical));
                            return result;

                        }
                    }

                    var config = await this.ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, GetMarketplace()));

                    var productStateResult = await this.GetProductIntegrationStatus(serviceMessage.Data.Data.AsMarketplaceServiceMessage(identity, config.Data));

                    if (!productStateResult.IsValid)
                    {
                        result.WithErrors(productStateResult.Errors);

                        return result;
                    }

                    var handleResult = await this.HandleMarketplaceProductState(identity, config.Data, serviceMessage.Data, productStateResult.Data);

                    if (!handleResult.IsValid)
                    {
                        result.WithErrors(handleResult.Errors);
                    }

                    if (result.IsValid)
                    {
                        await scope.PersistTimestampKeyAsync(new ServiceMessage<LockState>(identity, scope));
                    }
                }
            }
            catch(Exception ex) 
            { 
                
            }

            return result;
            #endregion
        }

        #endregion

        #region Broker

        protected virtual async Task<ServiceMessage> ConsumeCommand<T>(ServiceMessage<T> serviceMessage)
        {
            var result = ServiceMessage.CreateValidResult(serviceMessage.Identity);

            try
            {
                var messageBatches = this.BrokerService.PeekCommand<CommandMessage<T>>(new CommandMessage<T>(serviceMessage.Identity));

                foreach (var batch in messageBatches)
                {
                    var dequeueBatch = new DequeueCommandBatchMessage<T>();
                    dequeueBatch.Commands = new List<DequeueCommandMessage>();

                    // TODO: CREATE A TASK TO EACH MESSAGE AND USE WHEN ALL PER BATCH TO DEQUEUE
                    Parallel.ForEach(batch, async (message) =>
                    {
                        // consume
                        var consumeResult = this.Handlers[typeof(T)](message.Command.AsServiceMessage(message.Command.Identity)).GetAwaiter().GetResult();

                        // if succes add to dequeue array
                        if (consumeResult.IsValid)
                            dequeueBatch.Commands.Add(new DequeueCommandMessage() { Marketplace = GetMarketplace().ToString(), MessageId = message.MessageId, ReceiptHandle = message.ReceiptHandle });
                    });

                    if (dequeueBatch.Commands.Any())
                    {
                        var dequeueResult = this.BrokerService.DequeueCommandBatchAsync<T>(dequeueBatch.AsServiceMessage(serviceMessage.Identity)).GetAwaiter().GetResult();
                        result.WithErrors(dequeueResult.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to consume commands");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        #region Integration Monitor

        protected virtual async Task<ServiceMessage> HandleMarketplaceProductState<T>(Identity identity, AccountConfiguration configuration, CommandMessage<T> command, ProductIntegrationInfo productState)
        {
#region [Code]

            var message = new CommandMessage<ProductIntegrationInfo>(identity)
            {
                Data = productState,
                //CommandDataType = nameof(MarketplaceEntityState<Product>).ToLowerInvariant(),
                ServiceOperation = command.ServiceOperation,
                EventDateTime = command.EventDateTime,
                //ServiceOperation = command.ServiceOperation,
                CorrelationId = command.CorrelationId,
                Marketplace = configuration.Marketplace
            };

#if DEBUG
            await this.IntegrationMonitorService.HandleEntityState(message);
#endif
            await this.BrokerService.EnqueueCommandAsync(message);



            return ServiceMessage.CreateValidResult(identity);//TODO Implementar Enqueue no SQS
#endregion
        }

        protected virtual async Task<ServiceMessage> HandleMarketplacePriceState<T>(Identity identity, AccountConfiguration configuration, CommandMessage<T> command, PriceIntegrationInfo priceState)
        {
#region [Code]
            var message = new CommandMessage<PriceIntegrationInfo>(identity)
            {
                Data = priceState,
                //CommandDataType = nameof(MarketplaceEntityState<Product>).ToLowerInvariant(),
                ServiceOperation = command.ServiceOperation,
                EventDateTime = command.EventDateTime,
                //ServiceOperation = command.ServiceOperation,
                CorrelationId = command.CorrelationId,
                Marketplace = configuration.Marketplace
            };

#if DEBUG
            await this.IntegrationMonitorService.HandleEntityState(message);
#endif
            await this.BrokerService.EnqueueCommandAsync(message);

            return ServiceMessage.CreateValidResult(identity);//TODO Implementar Enqueue no SQS
#endregion
        }

        protected virtual async Task<ServiceMessage> HandleMarketplaceInventoryState<T>(Identity identity, AccountConfiguration configuration, CommandMessage<T> command, InventoryIntegrationInfo inventoryState)
        {
#region [Code]
            var message = new CommandMessage<InventoryIntegrationInfo>(identity)
            {
                Data = inventoryState,
                //CommandDataType = nameof(MarketplaceEntityState<Product>).ToLowerInvariant(),
                ServiceOperation = command.ServiceOperation,
                EventDateTime = command.EventDateTime,
                //ServiceOperation = command.ServiceOperation,
                CorrelationId = command.CorrelationId,
                Marketplace = configuration.Marketplace
            };

#if DEBUG
            await this.IntegrationMonitorService.HandleEntityState(message);
#endif
            await this.BrokerService.EnqueueCommandAsync(message);

            return ServiceMessage.CreateValidResult(identity);//TODO Implementar Enqueue no SQS
#endregion
        }

        #endregion

        #region [Private]
        protected static string GetCacheKey(string type, string id)
        {
            return $"{type}:{id}";
        }

        #endregion
    }
}

