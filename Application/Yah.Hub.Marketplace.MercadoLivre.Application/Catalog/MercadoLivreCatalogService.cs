using AutoMapper;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Data.Repositories.AnnouncementRepository;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Repositories.Announcement;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface;
using Yah.Hub.Marketplace.MercadoLivre.Application.Client;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement;
using System.Net;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Catalog
{
    public class MercadoLivreCatalogService : AbstractAnnouncementService, IMercadoLivreCatalogService
    {
        private IMercadoLivreClient Client { get; }

        public MercadoLivreCatalogService(
            ILogger<MercadoLivreCatalogService> logger, 
            IConfiguration configuration, 
            IAccountConfigurationService authentication, 
            IAnnouncementRepository announcementRepository,
            IAnnouncementSearchService announcementSearchService,
            IMercadoLivreClient client, 
            IBrokerService brokerService,
            IIntegrationMonitorService integrationMonitorService,
            IMarketplaceManifestService marketplaceManifestService,
            IValidationService validationService,
            ISecurityService securityService,
            ICacheService cacheService
            ) 
            : base(logger, configuration, authentication, announcementRepository, integrationMonitorService, announcementSearchService, brokerService, marketplaceManifestService, validationService, securityService, cacheService)
        {
            this.Client = client;
        }

        public override async Task<ServiceMessage<ProductIntegrationInfo>> GetProductIntegrationStatus(MarketplaceServiceMessage<RequestProductState> message)
        {
            var announcementState = new ProductIntegrationInfo(message.Data.Id, message.Data.ReferenceId, EntityStatus.Unknown, DateTimeOffset.UtcNow) { IntegrationId = message.Data.IntegrationId};

            var requestResult = await this.Client.GetMeliAnnouncement(message.Data.IntegrationId.AsMarketplaceServiceMessage(message));

            this.HandleAnnouncementClientResult(announcementState, requestResult);

            return ServiceMessage<ProductIntegrationInfo>.CreateValidResult(message.Identity, announcementState);
        }

        public async override Task<ServiceMessage<AnnouncementWrapper>> SendAnnouncementToMarketplace(MarketplaceServiceMessage<CommandMessage<Domain.Announcement.Announcement>> serviceMessage)
        {
            #region [Code]

            switch (serviceMessage.Data.ServiceOperation)
            {
                case Operation.Insert:
                    var requestCreateAnnoucement = serviceMessage.Data;
                    return await this.CreateMarketplaceAnnouncement(requestCreateAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
                    break;
                case Operation.Update:
                    var requestUpdateAnnoucement = serviceMessage.Data;
                    return await this.UpdateMarketplaceAnnouncement(requestUpdateAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
                    break;
                case Operation.Delete:
                    var requestDeleteAnnoucement = serviceMessage.Data;
                    return await this.DeleteMarketplaceAnnouncement(requestDeleteAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }

            #endregion
        }

        public async override Task<ServiceMessage<AnnouncementWrapper>> SendAnnouncementPriceToMarketplace(MarketplaceServiceMessage<CommandMessage<AnnouncementPrice>> serviceMessage)
        {
            return await this.UpdateMarketplacePriceAnnouncement(serviceMessage.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
        }

        public async override Task<ServiceMessage<AnnouncementWrapper>> SendAnnouncementInventoryToMarketplace(MarketplaceServiceMessage<CommandMessage<AnnouncementInventory>> serviceMessage)
        {
            return await this.UpdateMarketplaceInventoryAnnouncement(serviceMessage.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
        }

        public async Task<ServiceMessage<AnnouncementWrapper>> CreateMarketplaceAnnouncement(MarketplaceServiceMessage<CommandMessage<Announcement>> serviceMessage)
        {
            #region [Code]

            if (serviceMessage.Data == null)
            {
                return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(serviceMessage.Identity, new Error($"Necessário informar um anúncio para enviar ao marketplace {serviceMessage.Marketplace}","Propriedade Data está null",ErrorType.Technical),null);
            }

            serviceMessage.Identity.IsValidVendorTenantAccountIdentity();

            var message = serviceMessage.Data;

            var announcementState = new ProductIntegrationInfo(message.Data.Id, message.Data.Product.Id ?? message.Data.Product.IntegrationId, EntityStatus.Unknown, serviceMessage.Data.EventDateTime)
            { Name = serviceMessage.Data.Data.Item.Title ?? serviceMessage.Data.Data.Product.Name, Description = serviceMessage.Data.Data.Product.Description};

            var meliAnnoucement = Mapper.Map<MeliAnnouncement>(message.Data);

            this.SanitizeCreateAnnoucement(meliAnnoucement);
            meliAnnoucement.ItemId = null;

            var requestResult = await this.Client.CreateAnnoucement(meliAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            if(requestResult.Data != null && requestResult.IsValid)
            {
                meliAnnoucement.ItemId = requestResult.Data.ItemId;

                var descriptionResult = await this.Client.CreateAnnoucementDescription(meliAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                if (!descriptionResult.IsValid)
                {
                    base.Logger.LogError(descriptionResult.Errors.ToString());
                }
            }
            else
            {
                if (requestResult.Errors.Any())
                {
                    serviceMessage.Data.Data.Item.Status = EntityStatus.Declined;
                }
            }

            var annoucement = new Announcement(serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(),message.Data.Id);

            annoucement.MarketplaceId = requestResult?.Data?.ItemId ?? string.Empty;
            annoucement.Item = requestResult.IsValid ? Mapper.Map<AnnouncementItem>(requestResult.Data) : serviceMessage.Data.Data.Item;
            //annoucement.Id = serviceMessage.Data.Data.Id;
            annoucement.Product = message.Data.Product;
            annoucement.ProductId = message.Data.ProductId;
            annoucement.Timestamp = message.Data.Timestamp;
            annoucement.IsPausedByMeli = annoucement.Item.Status == EntityStatus.Stopped && annoucement.Item.SubStatus != null && annoucement.Item.SubStatus.Any()
                                ? annoucement.Item.SubStatus.Contains("picture_downloading_pending") || annoucement.Item.SubStatus.Contains("out_of_stock")
                                : false;

            announcementState.IntegrationId = requestResult?.Data?.ItemId ?? string.Empty;

            this.HandleAnnouncementClientResult(announcementState, requestResult);

            return ServiceMessage<AnnouncementWrapper>.CreateValidResult(serviceMessage.Identity, new AnnouncementWrapper(annoucement, announcementState));
            #endregion
        }

        public async Task<ServiceMessage> ResyncAnnouncement(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            var result = new ServiceMessage(marketplaceServiceMessage.Identity);

            try
            {
                var announcementResult = await this.Client.GetMeliAnnouncement(marketplaceServiceMessage);
                if (!announcementResult.IsValid)
                {
                    result.WithErrors(announcementResult.Errors);
                    return result;
                }

                var currentAnnouncementResult = await this.AnnouncementRepository.GetItemByMarketplaceId(marketplaceServiceMessage.Data.AsServiceMessage(marketplaceServiceMessage.Identity));
                if (!currentAnnouncementResult.IsValid)
                {
                    result.WithErrors(currentAnnouncementResult.Errors);
                    return result;
                }
                if (currentAnnouncementResult.Data == null || !currentAnnouncementResult.Data.Any())
                    result.WithError(new Error($"Error while resync Announcement: {marketplaceServiceMessage.Data}", "Announcement not found on repository", ErrorType.Business));

                var currentAnnouncement = currentAnnouncementResult.Data.First();
                var mappedAnnouncement = Mapper.Map<AnnouncementItem>(announcementResult.Data);
                currentAnnouncement.Item = mappedAnnouncement;
                currentAnnouncement.IsActive = (mappedAnnouncement.Status != EntityStatus.Stopped && mappedAnnouncement.Status != EntityStatus.Closed && mappedAnnouncement.Status != EntityStatus.Declined);
                currentAnnouncement.IsPausedByMeli = currentAnnouncement.Item.Status == EntityStatus.Stopped && currentAnnouncement.Item.SubStatus != null && currentAnnouncement.Item.SubStatus.Any()
                                            ? currentAnnouncement.Item.SubStatus.Contains("picture_download_pending") || currentAnnouncement.Item.SubStatus.Contains("out_of_stock")
                                            : false;

                var upsertResult = await this.AnnouncementRepository.UpsertItem(currentAnnouncement.AsServiceMessage(marketplaceServiceMessage.Identity));

                if (!upsertResult.IsValid)
                {
                    result.WithErrors(upsertResult.Errors);
                    return result;
                }
                var replicateResult = await this.ReplicateAnnouncementById(currentAnnouncement.Id.AsMarketplaceServiceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration));
                result.WithErrors(replicateResult.Errors);
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
                Logger.LogCustomCritical(new Error(ex), marketplaceServiceMessage.Identity);
            }
           
            return result; 
        }

        public async Task<ServiceMessage<AnnouncementWrapper>> DeleteMarketplaceAnnouncement(MarketplaceServiceMessage<CommandMessage<Domain.Announcement.Announcement>> serviceMessage)
        {
            #region [Code]

            if (serviceMessage.Data == null)
            {
                return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(serviceMessage.Identity, new Error($"Necessário informar um anúncio para enviar ao marketplace {serviceMessage.Marketplace}", "Propriedade Data está null", ErrorType.Technical), null);
            }

            serviceMessage.Identity.IsValidVendorTenantAccountIdentity();
            
            var message = serviceMessage.Data;
            
            var announcementState = new ProductIntegrationInfo(message.Data.Id, message.Data.Product.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var updateStatus = new UpdateAnnoucementStatus()
            {
                Status = AnnouncementStatus.Closed,
                SubStatus = "deleted"
            };

            var requestResult = await this.Client.SetAnnoucementStatus(updateStatus.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            if (requestResult.IsValid)
            {
                var deleteResult = await this.Client.DeleteAnnoucement(updateStatus.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                if (!deleteResult.IsValid)
                {
                    base.Logger.LogError(deleteResult.Errors.ToString());
                }
            }

            var annoucement = new Announcement(serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), message.Data.Id);

            annoucement.MarketplaceId = requestResult.Data.ItemId;
            annoucement.Item = Mapper.Map<AnnouncementItem>(requestResult.Data);
            annoucement.Product = message.Data.Product;

            this.HandleAnnouncementClientResult(announcementState, requestResult);

            return ServiceMessage<AnnouncementWrapper>.CreateValidResult(serviceMessage.Identity, new AnnouncementWrapper(annoucement, announcementState));

            #endregion
        }

        public async Task<ServiceMessage<AnnouncementWrapper>> UpdateMarketplaceAnnouncement(MarketplaceServiceMessage<CommandMessage<Announcement>> serviceMessage)
        {
            #region [Code]
            if (serviceMessage.Data == null)
            {
                return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(serviceMessage.Identity, new Error($"Necessário informar um anúncio para enviar ao marketplace {serviceMessage.Marketplace}", "Propriedade Data está null", ErrorType.Technical), null);
            }
            serviceMessage.Identity.IsValidVendorTenantAccountIdentity();

            var message = serviceMessage.Data;


            var announcementState = new ProductIntegrationInfo(message.Data.Id, message.Data.Product.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var meliAnnoucement = Mapper.Map<MeliAnnouncement>(message.Data);

            var currentMeliAnnouncement = await this.Client.GetMeliAnnouncement(serviceMessage.Data.Data.MarketplaceId.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            if (currentMeliAnnouncement.IsValid)
            {
                if (currentMeliAnnouncement.Data.Status.Equals(AnnouncementStatus.UnderReview) && currentMeliAnnouncement.Data.SubStatus.Contains("forbidden"))
                {
                    var annoucementReview = new Announcement(serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Data.Data.Id);

                    annoucementReview.MarketplaceId = String.IsNullOrEmpty(currentMeliAnnouncement?.Data?.ItemId)
                                                ? !string.IsNullOrWhiteSpace(serviceMessage.Data.Data.MarketplaceId) && serviceMessage.Data.Data.MarketplaceId != "0"
                                                    ? serviceMessage.Data.Data.MarketplaceId
                                                    : String.Empty
                                                : currentMeliAnnouncement?.Data?.ItemId;

                    annoucementReview.Item = currentMeliAnnouncement.IsValid ? Mapper.Map<AnnouncementItem>(currentMeliAnnouncement.Data) : serviceMessage.Data.Data.Item;
                    annoucementReview.Product = message.Data.Product;
                    annoucementReview.ProductId = message.Data.ProductId;
                    annoucementReview.Timestamp = message.Data.Timestamp;

                    var infractionsResult = await this.Client.GetMeliInfractionsByAnnouncementId(serviceMessage.Data.Data.MarketplaceId.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                    if (infractionsResult.IsValid)
                    {
                        var infraction = infractionsResult.Data.Infractions.FirstOrDefault();
                        
                        if(infraction.FilterGroup == "DOMAIN")
                        {
                            var suggestCategory = infraction.Suggested.SuggestCategory.FirstOrDefault().Path;

                            var errormessage = $"{infraction.Remedy} {infraction.Reason} {suggestCategory}";

                            announcementState.Errors.Add(new IntegrationError() { ErrorMessage = errormessage });

                        }
                        else
                        {
                            announcementState.Errors.Add(new IntegrationError() { ErrorMessage = infraction.Remedy });
                        }
                    }

                    announcementState.IntegrationActionResult = IntegrationActionResult.Continue;
                    announcementState.Status = EntityStatus.Stopped;

                    return ServiceMessage<AnnouncementWrapper>.CreateValidResult(serviceMessage.Identity, new AnnouncementWrapper(annoucementReview, announcementState));
                }
            }

            this.SanitizeUpdateAnnoucement(meliAnnoucement);

            var meliDescription = meliAnnoucement.Description;

            var requestResult = await this.Client.UpdateAnnoucement(meliAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            if (requestResult.Data != null && requestResult.IsValid)
            {
                meliAnnoucement.ItemId = requestResult.Data.ItemId;
                meliAnnoucement.Description = meliDescription;

                var descriptionResult = await this.Client.UpdateAnnoucementDescription(meliAnnoucement.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                if (!descriptionResult.IsValid)
                {
                    base.Logger.LogError(descriptionResult.Errors.ToString());
                }
            }
            else
            {
                if (requestResult.Errors.Any())
                {
                    if(!string.IsNullOrWhiteSpace(serviceMessage.Data.Data.MarketplaceId) && serviceMessage.Data.Data.MarketplaceId != "0")
                    {                        
                        serviceMessage.Data.Data.Item = Mapper.Map<AnnouncementItem>(currentMeliAnnouncement.Data);
                    }
                    else
                    {
                        serviceMessage.Data.Data.Item.Status = EntityStatus.Declined;
                    }
                }
            }

            var annoucement = new Announcement(serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Data.Data.Id);

            annoucement.MarketplaceId = String.IsNullOrEmpty(requestResult?.Data?.ItemId) 
                                        ? !string.IsNullOrWhiteSpace(serviceMessage.Data.Data.MarketplaceId) && serviceMessage.Data.Data.MarketplaceId != "0" 
                                            ? serviceMessage.Data.Data.MarketplaceId
                                            : String.Empty
                                        : requestResult?.Data?.ItemId;

            annoucement.Item = requestResult.IsValid? Mapper.Map<AnnouncementItem>(requestResult.Data) : serviceMessage.Data.Data.Item;
            annoucement.Product = message.Data.Product;
            annoucement.ProductId = message.Data.ProductId;
            annoucement.Timestamp = message.Data.Timestamp;
            annoucement.IsPausedByMeli = annoucement.Item.Status == EntityStatus.Stopped && annoucement.Item.SubStatus != null && annoucement.Item.SubStatus.Any() 
                                            ? annoucement.Item.SubStatus.Contains("picture_download_pending") || annoucement.Item.SubStatus.Contains("out_of_stock")
                                            : false;

            announcementState.IntegrationId = requestResult?.Data?.ItemId ?? string.Empty;

            this.HandleAnnouncementClientResult(announcementState, requestResult);

            return ServiceMessage<AnnouncementWrapper>.CreateValidResult(serviceMessage.Identity, new AnnouncementWrapper(annoucement, announcementState));
            #endregion
        }

        public async override Task<ServiceMessage<AnnouncementWrapper>> ChangeMarketplaceAnnouncementState(MarketplaceServiceMessage<(CommandMessage<Announcement> announcement, AnnouncementState state)> serviceMessage)
        {
            #region [Code]

            if (serviceMessage.Data.announcement == null)
            {
                return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(serviceMessage.Identity, new Error($"Necessário informar um anúncio para enviar ao marketplace {serviceMessage.Marketplace}", "Propriedade Data está null", ErrorType.Technical), null);
            }

            serviceMessage.Identity.IsValidVendorTenantAccountIdentity();
            var updateStatus = new UpdateAnnoucementStatus()
            {
                Status = AnnouncementStatus.Active,
            };


            var message = serviceMessage.Data.announcement;

            switch (serviceMessage.Data.state)
            {
                case AnnouncementState.Active:
                    updateStatus.Status = AnnouncementStatus.Active;
                    break;
                case AnnouncementState.Paused:
                    updateStatus.Status = AnnouncementStatus.Paused;
                    break;
                case AnnouncementState.Closed:
                    updateStatus.Status = AnnouncementStatus.Closed;
                    break;
                default:
                    break;
            }

            var announcementState = new ProductIntegrationInfo(message.Data.Id, message.Data.Product.Id, EntityStatus.Unknown, message.EventDateTime);

            var requestResult = await this.Client.SetAnnoucementStatus(updateStatus.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            var annoucement = new Announcement(serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), message.Data.Id);

            annoucement.MarketplaceId = requestResult.Data.ItemId;
            annoucement.Item = Mapper.Map<AnnouncementItem>(requestResult.Data);
            annoucement.Product = message.Data.Product;

            this.HandleAnnouncementClientResult(announcementState, requestResult);

            return ServiceMessage<AnnouncementWrapper>.CreateValidResult(serviceMessage.Identity, new AnnouncementWrapper(annoucement, announcementState));

            #endregion
        }

        public async Task<ServiceMessage<AnnouncementWrapper>> UpdateMarketplaceInventoryAnnouncement(MarketplaceServiceMessage<CommandMessage<AnnouncementInventory>> message)
        {
            #region [Code]
            if (message.Data == null)
            {
                return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(message.Identity, new Error($"Necessário informar um anúncio para enviar ao marketplace {message.Marketplace}", "Propriedade Data está null", ErrorType.Technical), null);
            }
           
            var result = ServiceMessage<AnnouncementWrapper>.CreateValidResult(message.Identity);

            message.Identity.IsValidVendorTenantAccountIdentity();

            var announcementPrice = message.Data.Data.Announcement;
            var affectedSku = message.Data.Data.AffectedSku;

            var announcementInventoryState = new InventoryIntegrationInfo()
            {
                Id = announcementPrice.Id,
                ReferenceId = announcementPrice.ProductId,
                Balance = affectedSku.Inventory.Balance,
                HandlingDays = affectedSku.Inventory.HandlingDays,
                Status = EntityStatus.Unknown,
                DateTime = message.Data.EventDateTime
            };

            var meliInventory = Mapper.Map<AnnouncementInventory, MeliAnnoucementInventory>(message.Data.Data);

            var requestResult = await this.Client.UpdateAnnoucementInventory(meliInventory.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if(requestResult != null && !requestResult.IsValid)
            {
                if(requestResult.Errors != null)
                {
                    result.WithErrors(requestResult.Errors);

                    return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(message.Identity, result.Errors, null);
                }
            }

            message.Data.Data.Announcement.Item = Mapper.Map<AnnouncementItem>(requestResult.Data);

            this.HandleAnnouncementInventoryClientResult(announcementInventoryState, requestResult);

            result.WithData(new AnnouncementWrapper(message.Data.Data.Announcement, announcementInventoryState));

            return result;
            #endregion
        }

        public async Task<ServiceMessage<AnnouncementWrapper>> UpdateMarketplacePriceAnnouncement(MarketplaceServiceMessage<CommandMessage<AnnouncementPrice>> message)
        {
            #region [Code]
            if (message.Data == null)
            {
                return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(message.Identity, new Error($"Necessário informar um anúncio para enviar ao marketplace {message.Marketplace}", "Propriedade Data está null", ErrorType.Technical), null);
            }
            var result = ServiceMessage<AnnouncementWrapper>.CreateValidResult(message.Identity);

            message.Identity.IsValidVendorTenantAccountIdentity();

            var announcementPrice = message.Data.Data.Announcement;
            var affectedSku = message.Data.Data.AffectedSku;

            var announcementPriceState = new PriceIntegrationInfo()
            {
                Id = announcementPrice.Id,
                ReferenceId = announcementPrice.ProductId,
                List = affectedSku.Price.List,
                Retail = affectedSku.Price.Retail,
                SalePrice = affectedSku.Price.SalePrice,
                SalePriceTo = affectedSku.Price.SalePriceTo,
                Status = EntityStatus.Unknown,
                DateTime = message.Data.EventDateTime
            };

            var meliPrice = Mapper.Map<AnnouncementPrice, MeliAnnoucementPrice>(message.Data.Data);

            var requestResult = await this.Client.UpdateAnnoucementPrice(meliPrice.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (requestResult != null && !requestResult.IsValid)
            {
                if (requestResult.Errors != null)
                {
                    result.WithErrors(requestResult.Errors);

                    return ServiceMessage<AnnouncementWrapper>.CreateInvalidResult(message.Identity, result.Errors, null);
                }
            }

            message.Data.Data.Announcement.Item = Mapper.Map<AnnouncementItem>(requestResult.Data);

            this.HandleAnnouncementPriceClientResult(announcementPriceState, requestResult);

            result.WithData(new AnnouncementWrapper(message.Data.Data.Announcement, announcementPriceState));

            return result;
            #endregion
        }



        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }

        #region [Handlers]

        private void HandleAnnouncementClientResult(ProductIntegrationInfo announcementState, HttpMarketplaceMessage<MeliAnnouncement> message)
        {
            #region [Code]

            switch (message.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    announcementState.IntegrationActionResult = IntegrationActionResult.Continue;
                    announcementState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Unauthorized:
                    announcementState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    announcementState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    announcementState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementState.Status = EntityStatus.Unknown;
                    announcementState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    announcementState.Status =  EntityStatus.Unknown;
                    announcementState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    announcementState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementState.IntegrationActionResult = IntegrationActionResult.Continue;
                    announcementState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    break;

                default:
                    announcementState.Status = EntityStatus.Unknown;
                    announcementState.IntegrationActionResult = IntegrationActionResult.Continue;
                    announcementState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    break;
            }
            #endregion
        }

        private void HandleAnnouncementPriceClientResult(PriceIntegrationInfo announcementPriceState, HttpMarketplaceMessage<MeliAnnouncement> message)
        {
            #region [Code]

            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    announcementPriceState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    announcementPriceState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementPriceState.Status = this.GetEntityStatus(message.Data.Status);
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    announcementPriceState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementPriceState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementPriceState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    announcementPriceState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementPriceState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    announcementPriceState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementPriceState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    announcementPriceState.Status = EntityStatus.Unknown;
                    announcementPriceState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }
            #endregion
        }

        private void HandleAnnouncementInventoryClientResult(InventoryIntegrationInfo announcementInventoryState, HttpMarketplaceMessage<MeliAnnouncement> message)
        {
            #region [Code]

            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    announcementInventoryState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    announcementInventoryState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementInventoryState.Status = this.GetEntityStatus(message.Data.Status);
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    announcementInventoryState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    announcementInventoryState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementInventoryState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    announcementInventoryState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementInventoryState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    announcementInventoryState.Status = this.GetEntityStatus(message.Data.Status);
                    announcementInventoryState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    announcementInventoryState.Status = EntityStatus.Unknown;
                    announcementInventoryState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }
            #endregion
        }

        #endregion

        #region [Private]

        private EntityStatus GetEntityStatus(AnnouncementStatus status)
        {
            #region [Code]
            switch (status)
            {
                case AnnouncementStatus.Active:
                    return EntityStatus.Accepted;
                case AnnouncementStatus.Paused:
                    return EntityStatus.Stopped;
                case AnnouncementStatus.Inactive:
                    return EntityStatus.Closed;
                case AnnouncementStatus.UnderReview:
                    return EntityStatus.PendingValidation;
                default:
                    return EntityStatus.Unknown;
            }
            #endregion
        }

        private void SanitizeCreateAnnoucement(MeliAnnouncement announcement)
        {
            announcement.SubStatus = null;
        }

        private void SanitizeUpdateAnnoucement(MeliAnnouncement announcement)
        {
            announcement.ListingType = null;
            announcement.BuyingMode = null;
            announcement.CategoryId = null;
            announcement.Condition = null;
            announcement.PermaLink = null;
            announcement.Sold = null;
            announcement.SubStatus = null;

            if (announcement.Shipping.ShippingMode.ToLower() == "me2")
            {
                announcement.Shipping.Dimensions = null;
                announcement.Shipping.Tags = null;
            }
        }

        #endregion
    }
}
