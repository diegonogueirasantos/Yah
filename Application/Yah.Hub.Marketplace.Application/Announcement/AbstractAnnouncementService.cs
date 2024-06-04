using System;
using Nest;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Repositories.Announcement;
using Yah.Hub.Domain.Order;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Data.Repositories.AnnouncementRepository;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Enums;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Yah.Hub.Common.ChannelConfiguration;
using Identity = Yah.Hub.Common.Identity.Identity;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.Services.CacheService;

namespace Yah.Hub.Marketplace.Application.Announcement
{
    public abstract class AbstractAnnouncementService : AbstractCatalogService, IAnnouncementService
    {
        protected readonly IAnnouncementRepository AnnouncementRepository;
        private readonly IAnnouncementSearchService AnnouncementSearchService;

        public AbstractAnnouncementService(
            ILogger<AbstractAnnouncementService> logger, 
            IConfiguration configuration, 
            IAccountConfigurationService authentication, 
            IAnnouncementRepository announcementRepository, 
            IIntegrationMonitorService integrationMonitorService,
            IAnnouncementSearchService announcementSearchService,
            IBrokerService brokerService,
            IMarketplaceManifestService marketplaceManifestService,
            IValidationService validationService,
            ISecurityService securityService,
            ICacheService cacheService) 
            : base(configuration, logger, authentication, integrationMonitorService, brokerService, marketplaceManifestService, validationService, securityService, cacheService)
        {
            this.AnnouncementRepository = announcementRepository;
            this.AnnouncementSearchService = announcementSearchService;

            if (Handlers == null)
                Handlers = new Dictionary<Type, Func<dynamic, Task<ServiceMessage>>>();

            // announcement
            this.Handlers.TryAdd(typeof(Domain.Announcement.Announcement), (dynamic payload) => this.ExecuteAnnouncementCommand(payload));
            // announcement price
            this.Handlers.TryAdd(typeof(Domain.Announcement.AnnouncementPrice), (dynamic payload) => this.ExecuteAnnouncementPriceCommand(payload));
            // announcement inventory
            this.Handlers.TryAdd(typeof(Domain.Announcement.AnnouncementInventory), (dynamic payload) => this.ExecuteAnnouncementInventoryCommand(payload));
        }

        #region Abstract

        public abstract Task<ServiceMessage<AnnouncementWrapper>> SendAnnouncementToMarketplace(MarketplaceServiceMessage<CommandMessage<Domain.Announcement.Announcement>> marketplaceServiceMessage);
        public abstract Task<ServiceMessage<AnnouncementWrapper>> SendAnnouncementPriceToMarketplace(MarketplaceServiceMessage<CommandMessage<AnnouncementPrice>> marketplaceServiceMessage);
        public abstract Task<ServiceMessage<AnnouncementWrapper>> SendAnnouncementInventoryToMarketplace(MarketplaceServiceMessage<CommandMessage<AnnouncementInventory>> serviceMessage);
        public abstract Task<ServiceMessage<AnnouncementWrapper>> ChangeMarketplaceAnnouncementState(MarketplaceServiceMessage<(CommandMessage<Domain.Announcement.Announcement> announcement, AnnouncementState state)> serviceMessage);
        

        #endregion

            #region Consumers

        public virtual async Task<ServiceMessage> ConsumeAnnouncementCommand(ServiceMessage serviceMessage)
        {
            return await this.ConsumeCommand(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity));
        }

        public virtual async Task<ServiceMessage> ConsumeAnnouncementPriceCommand(ServiceMessage serviceMessage)
        {
            return await this.ConsumeCommand(new ServiceMessage<Domain.Announcement.AnnouncementPrice>(serviceMessage.Identity));
        }

        public virtual async Task<ServiceMessage> ConsumeAnnouncementInventoryCommand(ServiceMessage serviceMessage)
        {
            return await this.ConsumeCommand(new ServiceMessage<Domain.Announcement.AnnouncementInventory>(serviceMessage.Identity));
        }

        #endregion

        #region Producers

        public virtual async Task<ServiceMessage> EnqueueAnnouncementCommand(ServiceMessage<CommandMessage<Domain.Announcement.Announcement>> serviceMessage)
        {
            return await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.Announcement>(serviceMessage.Data);
        }

        public virtual async Task<ServiceMessage> EnqueueAnnouncementPriceCommand(ServiceMessage<CommandMessage<Domain.Announcement.AnnouncementPrice>> serviceMessage)
        {
            return await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.AnnouncementPrice>(serviceMessage.Data);
        }

        public virtual async Task<ServiceMessage> EnqueueAnnouncementInventoryCommand(ServiceMessage<CommandMessage<Domain.Announcement.AnnouncementInventory>> serviceMessage)
        {
            return await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.AnnouncementInventory>(serviceMessage.Data);
        }

        #endregion

        #region Implementations

        public override async Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            try
            {
                var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();
                result.Data = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, Common.Enums.EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

                //find all announcements by product
                var announcementsResult = await GetAnnouncementsByProductId(new MarketplaceServiceMessage<string>(serviceMessage.Identity, serviceMessage.AccountConfiguration, serviceMessage.Data.Data.Id));
                if (!announcementsResult.IsValid)
                {
                    result.WithErrors(announcementsResult.Errors);
                    return result;
                }

                if (!announcementsResult.Data.Any())
                {
                    result.Data = null;
                    return result;
                }

                commandList = GenerateAnnouncementsCommands(new MarketplaceServiceMessage<(CommandMessage<Product> originCommand, List<Domain.Announcement.Announcement> announcements)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (serviceMessage.Data, announcementsResult.Data)));

                foreach(var announcement in commandList)
                {
                    await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.Announcement>(announcement);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while create announcement from product command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }


        public override async Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            try
            {

                var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();
                result.Data = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

                //find all announcements by product
                var announcementsResult = await GetAnnouncementsByProductId(new MarketplaceServiceMessage<string>(serviceMessage.Identity, serviceMessage.AccountConfiguration, serviceMessage.Data.Data.Id));
                if (!announcementsResult.IsValid)
                {
                    result.WithErrors(announcementsResult.Errors);
                    return result;
                }

                commandList = GenerateAnnouncementsCommands(new MarketplaceServiceMessage<(CommandMessage<Product> originCommand, List<Domain.Announcement.Announcement> announcements)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (serviceMessage.Data, announcementsResult.Data)));

                foreach (var announcement in commandList)
                {
                    await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.Announcement>(announcement);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while update announcement from product command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }


        public override async Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            try
            {

                var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();
                result.Data = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, Common.Enums.EntityStatus.Unknown, serviceMessage.Data.EventDateTime);
               

                //find all announcements by product
                var announcementsResult = await GetAnnouncementsByProductId(new MarketplaceServiceMessage<string>(serviceMessage.Identity, serviceMessage.AccountConfiguration, serviceMessage.Data.Data.Id));
                if (!announcementsResult.IsValid)
                {
                    result.WithErrors(announcementsResult.Errors);
                    return result;
                }

                commandList = GenerateAnnouncementsCommands(new MarketplaceServiceMessage<(CommandMessage<Product> originCommand, List<Domain.Announcement.Announcement> announcements)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (serviceMessage.Data, announcementsResult.Data)));

                foreach (var announcement in commandList)
                {
                    await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.Announcement>(announcement);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while delete announcement from product command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            throw new NotImplementedException();
        }

        public override async Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity);
            var marketplace = serviceMessage.AccountConfiguration.Marketplace;
            try
            {
                var skuCommand = new CommandMessage<Sku>(serviceMessage.Identity) { Marketplace = GetMarketplace()};
                skuCommand.Data = serviceMessage.Data.Data.AffectedSku;

                var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();

                //find all announcements by product
                var announcementsResult = await GetAnnouncementsByProductId(new MarketplaceServiceMessage<string>(serviceMessage.Identity, serviceMessage.AccountConfiguration, serviceMessage.Data.Data.AffectedSku.ProductId));
                if (!announcementsResult.IsValid)
                {
                    result.WithErrors(announcementsResult.Errors);
                    return result;
                }

                foreach (var announcement in announcementsResult.Data)
                {
                    var announcementInventory = new AnnouncementInventory() { AffectedSku = serviceMessage.Data.Data.AffectedSku, Announcement = announcement, Skus = serviceMessage.Data.Data.Skus, Id = serviceMessage.Data.Data.Id, IntegrationId = serviceMessage.Data.Data.IntegrationId, Name = serviceMessage.Data.Data.Name };
                    var commandMessage = new CommandMessage<AnnouncementInventory>(serviceMessage.Identity)
                    {
                        ServiceOperation = serviceMessage.Data.ServiceOperation,
                        CorrelationId = serviceMessage.Data.CorrelationId,
                        EventDateTime = serviceMessage.Data.EventDateTime,
                        ExecutionStep = serviceMessage.Data.ExecutionStep,
                        Marketplace = GetMarketplace(),
                        Data = announcementInventory
                    };

                    await this.ExecuteAnnouncementInventoryCommand(commandMessage.AsServiceMessage(serviceMessage.Identity));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while update inventory from product command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        public override async Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            var result = new ServiceMessage<PriceIntegrationInfo>(serviceMessage.Identity);
            var marketplace = serviceMessage.AccountConfiguration.Marketplace;
            try
            {
                var skuCommand = new CommandMessage<Sku>(serviceMessage.Identity) { Marketplace = GetMarketplace() };
                skuCommand.Data = serviceMessage.Data.Data.AffectedSku;

                var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();
                
                //find all announcements by product
                var announcementsResult = await GetAnnouncementsByProductId(new MarketplaceServiceMessage<string>(serviceMessage.Identity, serviceMessage.AccountConfiguration, serviceMessage.Data.Data.AffectedSku.ProductId));
                if (!announcementsResult.IsValid)
                {
                    result.WithErrors(announcementsResult.Errors);
                    return result;
                }

                foreach (var announcement in announcementsResult.Data)
                {
                    var announcementPrice = new AnnouncementPrice() { AffectedSku = serviceMessage.Data.Data.AffectedSku, Announcement = announcement, Skus = serviceMessage.Data.Data.Skus, Id = serviceMessage.Data.Data.Id, IntegrationId = serviceMessage.Data.Data.IntegrationId, Name = serviceMessage.Data.Data.Name };
                    var commandMessage = new CommandMessage<AnnouncementPrice>(serviceMessage.Identity)
                    {
                        ServiceOperation = serviceMessage.Data.ServiceOperation,
                        CorrelationId = serviceMessage.Data.CorrelationId,
                        EventDateTime = serviceMessage.Data.EventDateTime,
                        ExecutionStep = serviceMessage.Data.ExecutionStep,
                        Marketplace = GetMarketplace(),
                        Data = announcementPrice
                    };
                    
                    await this.ExecuteAnnouncementPriceCommand(commandMessage.AsServiceMessage(serviceMessage.Identity));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while update price from product command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        #region Private

        private async Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetAnnouncementsByProductId(MarketplaceServiceMessage<string> serviceMessage)
        {
            return await this.AnnouncementRepository.GetItemByProductId(new ServiceMessage<string>(serviceMessage.Identity, serviceMessage.Data));
        }

        private List<CommandMessage<Domain.Announcement.Announcement>> GenerateAnnouncementsCommands(MarketplaceServiceMessage<(CommandMessage<Product> originCommand, List<Domain.Announcement.Announcement> announcements)> serviceMessage)
        {
            var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();
            foreach (var announcement in serviceMessage.Data.announcements)
            {
                announcement.Product = serviceMessage.Data.originCommand.Data;
                var announcementCommand = new CommandMessage<Domain.Announcement.Announcement>(serviceMessage.Identity)
                {
                    CorrelationId = new Guid().ToString(),
                    EventDateTime = new DateTime(),
                    //CommandDataType = typeof(Domain.Announcement.Announcement).ToString(),
                    ServiceOperation = serviceMessage.Data.originCommand.ServiceOperation,
                    Marketplace = GetMarketplace()
                };

                commandList.Add(announcementCommand);
            }

            return commandList;
        }

        private List<CommandMessage<Domain.Announcement.Announcement>> GenerateAnnouncementsCommands(MarketplaceServiceMessage<(CommandMessage<Sku> originCommand, List<Domain.Announcement.Announcement> announcements)> serviceMessage)
        {
            // update skus
            foreach (var item in serviceMessage.Data.announcements)
            {
                item.Product.Skus.Remove(item.Product.Skus.FirstOrDefault(x => x.Id == serviceMessage.Data.originCommand.Data.Id));
                item.Product.Skus.Add(serviceMessage.Data.originCommand.Data);
            }

            var commandList = new List<CommandMessage<Domain.Announcement.Announcement>>();
            foreach (var announcement in serviceMessage.Data.announcements)
            {
                var announcementCommand = new CommandMessage<Domain.Announcement.Announcement>(serviceMessage.Identity)
                {
                    CorrelationId = new Guid().ToString(),
                    EventDateTime = new DateTime(),
                    //CommandDataType = typeof(Domain.Announcement.Announcement).ToString(),
                    ServiceOperation = serviceMessage.Data.originCommand.ServiceOperation,
                    Marketplace = GetMarketplace()
                };

                commandList.Add(announcementCommand);
            }

            return commandList;
        }

        #endregion

        #region Announcement Commands

        public async Task<ServiceMessage> ExecuteAnnouncementCommand(ServiceMessage<CommandMessage<Domain.Announcement.Announcement>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<AnnouncementWrapper>(serviceMessage.Identity);

            try
            {
                if (serviceMessage.Data.Data == null)
                {
                    result.WithError( new Error("Comando não possui Annoucement vinculado", "Necessário informar um Annoucement no comando", ErrorType.Technical));
                    return result;
                }

                var accountResult = await ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, this.GetMarketplace()));

                if (!accountResult.IsValid)
                {
                    result.WithErrors(accountResult.Errors);
                }

                serviceMessage.Data.Data.Timestamp = DateTime.Now;

                if (serviceMessage.Data.ServiceOperation == Operation.Insert)
                {
                    serviceMessage.Data.Data.Id = string.IsNullOrWhiteSpace(serviceMessage.Data.Data.Id) ? Guid.NewGuid().ToString() : serviceMessage.Data.Data.Id;
                    var createAnnouncement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, serviceMessage.Data.Data));
                }

                var sendResult = await this.SendAnnouncementToMarketplace(serviceMessage.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                if (sendResult.IsValid)
                {
                    var updateAnnoucement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, sendResult.Data.Announcement));

                    if (!updateAnnoucement.IsValid)
                        result.WithErrors(updateAnnoucement.Errors);

                    if (sendResult.Data != null)
                    {
                        if (sendResult.Data.IntegrationInfo.IntegrationActionResult == IntegrationActionResult.Retry)
                        {
                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {serviceMessage.Data.Data.Id}", "Throttling", ErrorType.Business));
                        }
                        else if (sendResult.Data.IntegrationInfo.IntegrationActionResult == IntegrationActionResult.NotAuthorized)
                        {
                            await this.DisableAccount(serviceMessage.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {serviceMessage.Data.Data.Id}", "Não Autorizado", ErrorType.Business));
                        }

                        var handleResult = await this.HandleMarketplaceAnnouncementState(sendResult.Data.IntegrationInfo, serviceMessage.Identity, accountResult.Data, serviceMessage.Data);

                        await this.ReplicateAnnouncementById(serviceMessage.Data.Data.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                        if (!handleResult.IsValid)
                        {
                            result.WithErrors(handleResult.Errors);
                        }
                    }
                }
                else
                {
                    result.WithErrors(sendResult.Errors);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while execute announcement command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<ServiceMessage> ExecuteAnnouncementPriceCommand(ServiceMessage<CommandMessage<AnnouncementPrice>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<AnnouncementWrapper>(serviceMessage.Identity);

            var announcementPrice = serviceMessage.Data.Data;

            try
            {
                var accountResult = await ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, this.GetMarketplace()));

                if (!accountResult.IsValid)
                {
                    result.WithErrors(accountResult.Errors);
                }

                if (announcementPrice == null)
                {
                    result.WithError(new Error("Comando não possui Annoucemente vinculado", "Necessário informar um Annoucemente no comando", ErrorType.Technical));
                    return result;
                }

                var sendResult = await this.SendAnnouncementPriceToMarketplace(serviceMessage.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                if (sendResult.IsValid)
                {
                    result.Data = sendResult.Data;
                    var updateAnnoucement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, sendResult.Data.Announcement));

                    if (!updateAnnoucement.IsValid)
                        result.WithErrors(updateAnnoucement.Errors);

                    if (sendResult.Data != null)
                    {
                        if (result.Data.PriceIntegrationInfo.IntegrationActionResult == IntegrationActionResult.Retry)
                        {
                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {serviceMessage.Data.Data.Id}", "Throttling", ErrorType.Business));
                        }
                        else if (result.Data.PriceIntegrationInfo.IntegrationActionResult == IntegrationActionResult.NotAuthorized)
                        {
                            await this.DisableAccount(serviceMessage.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {serviceMessage.Data.Data.Id}", "Não Autorizado", ErrorType.Business));
                        }
                        var handleResult = await this.HandleMarketplaceAnnouncementPriceState(result.Data.PriceIntegrationInfo, serviceMessage.Identity, accountResult.Data, serviceMessage.Data);

                        if (!handleResult.IsValid)
                        {
                            result.WithErrors(handleResult.Errors);
                        }
                    }
                }
                else
                {
                    result.WithErrors(sendResult.Errors);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while execute announcement price command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<ServiceMessage> ExecuteAnnouncementInventoryCommand(ServiceMessage<CommandMessage<AnnouncementInventory>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<AnnouncementWrapper>(serviceMessage.Identity);
            var announcementInventory = serviceMessage.Data.Data;

            try
            {
                var accountResult = await ConfigurationService.GetConfiguration(new MarketplaceServiceMessage(serviceMessage.Identity, this.GetMarketplace()));

                if (!accountResult.IsValid)
                {
                    result.WithErrors(accountResult.Errors);
                }

                if (announcementInventory == null)
                {
                    result.WithError(new Error("Comando não possui Annoucemente vinculado", "Necessário informar um Annoucemente no comando", ErrorType.Technical));
                    return result;
                }

                var sendResult = await this.SendAnnouncementInventoryToMarketplace(serviceMessage.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                if (sendResult.IsValid)
                {
                    result.Data = sendResult.Data;
                    var updateAnnoucement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, sendResult.Data.Announcement));

                    if (!updateAnnoucement.IsValid)
                        result.WithErrors(updateAnnoucement.Errors);

                    if (sendResult.Data != null)
                    {
                        if (result.Data.InventoryIntegrationInfo.IntegrationActionResult == IntegrationActionResult.Retry)
                        {
                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {serviceMessage.Data.Data.Id}", "Throttling", ErrorType.Business));
                        }
                        else if (result.Data.InventoryIntegrationInfo.IntegrationActionResult == IntegrationActionResult.NotAuthorized)
                        {
                            await this.DisableAccount(serviceMessage.AsMarketplaceServiceMessage(serviceMessage.Identity, accountResult.Data));

                            result.WithError(new Error($"Não foi possível processar o comando para o marketplace {GetMarketplace().ToString()} do produto {serviceMessage.Data.Data.Id}", "Não Autorizado", ErrorType.Business));
                        }
                        var handleResult = await this.HandleMarketplaceAnnouncementInventoryState(result.Data.InventoryIntegrationInfo, serviceMessage.Identity, accountResult.Data, serviceMessage.Data);

                        if (!handleResult.IsValid)
                        {
                            result.WithErrors(handleResult.Errors);
                        }
                    }
                }
                else
                {
                    result.WithErrors(sendResult.Errors);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while execute announcement inventory command");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        #endregion

        #region Announcement Management

        private async Task<ServiceMessage<Domain.Announcement.Announcement>> GetAnnonucementById(MarketplaceServiceMessage<string> serviceMessage)
        {
            var result = new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity);

            try
            {
                var announcementResult = await this.AnnouncementRepository.GetItemByPartitionKey(new ServiceMessage<string>(serviceMessage.Identity, $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}-{serviceMessage.Data}"));
                result = announcementResult;
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while get announcement by id");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }
            return result;
        }

        public async Task<ServiceMessage<Domain.Announcement.Announcement>> CreateAnnouncement(MarketplaceServiceMessage<Domain.Announcement.Announcement> serviceMessage)
        {
            var result = new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, serviceMessage.Data);
            if (serviceMessage.Data == null)
            {
                result.WithError(new Error("Announcement could not be null", "Announcement null", ErrorType.Business));
                return result;
            }

            try
            {
                result.Data.Id = Guid.NewGuid().ToString();
                result.Data.Item.Status = EntityStatus.Waiting;
                result.Data.Timestamp = DateTime.Now;

                var announcement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, result.Data));
                if (!announcement.IsValid)
                    result.WithErrors(announcement.Errors);

                result.Data.Id = serviceMessage.Data.Id;

                var announcementCommand = new CommandMessage<Domain.Announcement.Announcement>(serviceMessage.Identity)
                {
                    Data = result.Data,
                    ServiceOperation = Operation.Insert,
                    Marketplace = GetMarketplace(),
                    EventDateTime = DateTime.Now,
                    IsAnnouncement = true
                };
#if (DEBUG)
                await this.ExecuteAnnouncementCommand(announcementCommand.AsServiceMessage(serviceMessage.Identity));
#endif

                await this.BrokerService.EnqueueCommandAsync<Domain.Announcement.Announcement>(announcementCommand);

                await this.ReplicateAnnouncementById(result.Data.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while create announcement");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage<Domain.Announcement.Announcement>> GetAnnouncementById(MarketplaceServiceMessage<string> serviceMessage)
        {
            var result = new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity);

            try
            {
                var announcementResult = await this.AnnouncementRepository.GetItemById(new ServiceMessage<string>(serviceMessage.Identity, serviceMessage.Data));
                if (!announcementResult.IsValid)
                    result.WithErrors(announcementResult.Errors);

                result.Data = announcementResult.Data;
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while create announcement");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage<Domain.Announcement.Announcement>> UpdateAnnouncement(MarketplaceServiceMessage<Domain.Announcement.Announcement> serviceMessage)
        {
            var result = new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity);

            try
            {
                var announcement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, serviceMessage.Data));

                if (!announcement.IsValid)
                    result.WithErrors(announcement.Errors);

                var announcementCommand = new CommandMessage<Domain.Announcement.Announcement>(serviceMessage.Identity)
                {
                    Data = serviceMessage.Data,
                    ServiceOperation = Operation.Update,
                    Marketplace = GetMarketplace()
                };

                if(!string.IsNullOrWhiteSpace(serviceMessage.Data.MarketplaceId) && serviceMessage.Data.MarketplaceId != "0")
                {
                    await this.ExecuteAnnouncementCommand(announcementCommand.AsServiceMessage(serviceMessage.Identity));
                }
                else
                {
                    announcementCommand.ServiceOperation = Operation.Insert;
                    await this.ExecuteAnnouncementCommand(announcementCommand.AsServiceMessage(serviceMessage.Identity));
                }

                await this.ReplicateAnnouncementById(serviceMessage.Data.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while update announcement");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage<Domain.Announcement.Announcement>> DeleteAnnouncement(MarketplaceServiceMessage<string> serviceMessage)
        {
            var result = new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity);

            try
            {
                var currentAnnouncement = await this.AnnouncementRepository.GetItemById(serviceMessage.Data.AsServiceMessage(serviceMessage.Identity));
                if (!currentAnnouncement.IsValid)
                {
                    result.WithErrors(currentAnnouncement.Errors);
                    return result;
                }

                currentAnnouncement.Data.IsActive = false;
                currentAnnouncement.Data.IsDeleted = true;

                var announcement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, currentAnnouncement.Data));
                if (!announcement.IsValid)
                    result.WithErrors(announcement.Errors);

                var announcementCommand = new CommandMessage<Domain.Announcement.Announcement>(serviceMessage.Identity)
                {
                    Data = currentAnnouncement.Data,
                    ServiceOperation = Operation.Delete,
                    Marketplace = GetMarketplace()
                };

                await this.ExecuteAnnouncementCommand(announcementCommand.AsServiceMessage(serviceMessage.Identity));
                await this.ReplicateAnnouncementById(currentAnnouncement.Data.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while delete announcement");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage> ChangeAnnouncementState(MarketplaceServiceMessage<(string announcementId, AnnouncementState state)> serviceMessage)
        {
            var result = new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity);

            var getAnnouncementResult = await this.AnnouncementRepository.GetItemById(new ServiceMessage<string>(serviceMessage.Identity, serviceMessage.Data.announcementId));

            if (!getAnnouncementResult.IsValid)
            {
                result.WithErrors(getAnnouncementResult.Errors);
                return result;
            }

            result.Data = getAnnouncementResult.Data;

            try
            {
               
                switch (serviceMessage.Data.state)
                {
                    case AnnouncementState.Active:
                        result.Data.IsActive = true;
                        break;
                    case AnnouncementState.Paused:
                        result.Data.Item.Status = EntityStatus.Paused;
                        break;
                    case AnnouncementState.Closed:
                        result.Data.Item.Status = EntityStatus.Closed;
                        break;
                    default:
                        break;
                }

                var announcement = await this.AnnouncementRepository.UpsertItem(new ServiceMessage<Domain.Announcement.Announcement>(serviceMessage.Identity, result.Data));
                if (!announcement.IsValid)
                    result.WithErrors(announcement.Errors);

                var announcementCommand = new CommandMessage<Domain.Announcement.Announcement>(serviceMessage.Identity)
                {
                    Data = result.Data,
                    ServiceOperation = Operation.Update,
                    Marketplace = GetMarketplace()
                };

                await this.ChangeMarketplaceAnnouncementState((announcementCommand, serviceMessage.Data.state).AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
                await this.ReplicateAnnouncementById(result.Data.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while update announcement state");
                Logger.LogCustomCritical(error, serviceMessage.Identity, serviceMessage.Data);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        #region Replication

        public async Task<ServiceMessage> ReplicateAllAnnouncement(MarketplaceServiceMessage message)
        {
            var result = ServiceMessage.CreateValidResult(message.Identity);

            try
            {
                var config = await base.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, message.Marketplace));
                var announcementList = await this.AnnouncementRepository.GetAllAnnouncementsByAccount(message);
                var enumerable =  announcementList.Data
                                    .Chunk(50)
                                    .Select(x => x);

                foreach(var announcement in enumerable)
                {
                    result = await this.AnnouncementSearchService.SaveBulkAnnouncement(announcement.ToList().AsMarketplaceServiceMessage(message.Identity, config.Data));
                }
            }

            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while replicate announcements");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage> ReplicateAnnouncementById(MarketplaceServiceMessage<string> message)
        {
            var result = ServiceMessage.CreateValidResult(message.Identity);

            try
            {
                var config = await base.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, message.Marketplace));
                var announcement = await this.AnnouncementRepository.GetItemById(new ServiceMessage<string>(message.Identity, message.Data));
                result = await this.AnnouncementSearchService.SaveAnnouncement(announcement.Data.AsMarketplaceServiceMessage(message.Identity, config.Data));   
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while replicate announcement by id");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage> ReplicateAnnouncement(MarketplaceServiceMessage<Domain.Announcement.Announcement> message)
        {
            var result = ServiceMessage.CreateValidResult(message.Identity);

            try
            {
                var config = await base.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, message.Marketplace));
                result = await this.AnnouncementSearchService.SaveAnnouncement(message.Data.AsMarketplaceServiceMessage(message.Identity, config.Data));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while replicate announcement");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage<AnnouncementSearchResult>> QueryAsync(MarketplaceServiceMessage<AnnouncementQuery> message)
        {
            message = message ?? throw new ArgumentNullException(nameof(message));

            return await this.AnnouncementSearchService.QueryAsync(message);
        }

        #endregion

        #region [Handlers]

        protected virtual async Task<ServiceMessage> HandleMarketplaceAnnouncementState(ProductIntegrationInfo announcementState, Identity identity, AccountConfiguration configuration, CommandMessage<Domain.Announcement.Announcement> command)
        {
            #region [Code]
            var message = new CommandMessage<ProductIntegrationInfo>(identity)
            {
                Data = announcementState,
                //CommandDataType = nameof(MarketplaceEntityState<Product>).ToLowerInvariant(),
                ServiceOperation = command.ServiceOperation,
                EventDateTime = command.EventDateTime,
                //ServiceOperation = command.ServiceOperation,
                CorrelationId = command.CorrelationId,
                Marketplace = GetMarketplace()
            };

#if DEBUG
            await this.IntegrationMonitorService.HandleEntityState(message);
#endif
            await this.BrokerService.EnqueueCommandAsync(message);

            return ServiceMessage.CreateValidResult(identity);
            #endregion
        }

        protected virtual async Task<ServiceMessage> HandleMarketplaceAnnouncementPriceState(PriceIntegrationInfo announcementPriceState, Identity identity, AccountConfiguration configuration, CommandMessage<AnnouncementPrice> command)
        {
            #region [Code]
            var message = new CommandMessage<PriceIntegrationInfo>(identity)
            {
                Data = announcementPriceState,
                //CommandDataType = nameof(MarketplaceEntityState<Product>).ToLowerInvariant(),
                ServiceOperation = command.ServiceOperation,
                EventDateTime = command.EventDateTime,
                //ServiceOperation = command.ServiceOperation,
                CorrelationId = command.CorrelationId,
                Marketplace = GetMarketplace()
            };

#if DEBUG
            await this.IntegrationMonitorService.HandleEntityState(message);
#endif
            await this.BrokerService.EnqueueCommandAsync(message);

            return ServiceMessage.CreateValidResult(identity);
            #endregion
        }

        protected virtual async Task<ServiceMessage> HandleMarketplaceAnnouncementInventoryState(InventoryIntegrationInfo announcementInventoryState, Identity identity, AccountConfiguration configuration, CommandMessage<AnnouncementInventory> command)
        {
            #region [Code]
            var message = new CommandMessage<InventoryIntegrationInfo>(identity)
            {
                Data = announcementInventoryState,
                //CommandDataType = nameof(MarketplaceEntityState<Product>).ToLowerInvariant(),
                ServiceOperation = command.ServiceOperation,
                EventDateTime = command.EventDateTime,
                //ServiceOperation = command.ServiceOperation,
                CorrelationId = command.CorrelationId,
                Marketplace = GetMarketplace()
            };

#if DEBUG
            await this.IntegrationMonitorService.HandleEntityState(message);
#endif
            await this.BrokerService.EnqueueCommandAsync(message);

            return ServiceMessage.CreateValidResult(identity);
            #endregion
        }

        #endregion
    }
}

