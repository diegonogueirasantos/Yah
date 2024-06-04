using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Client.Interface;
using Yah.Hub.Marketplace.B2W.Application.Mappings;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Product = Yah.Hub.Marketplace.B2W.Application.Models.Product;
using Variation = Yah.Hub.Marketplace.B2W.Application.Models.Variation;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.B2W.Application.Catalog.Interface;
using Yah.Hub.Common.Services.CacheService;

namespace Yah.Hub.Marketplace.B2W.Application.Catalog
{
    public class B2WCatalogService : AbstractCatalogService, IB2WCatalogService
    {
        private IB2WClient Client { get; }
        

        public B2WCatalogService(
            IConfiguration configuration,
            ILogger<AbstractCatalogService> logger,
            IAccountConfigurationService configurationService,
            IB2WClient client,
            IIntegrationMonitorService integrationMonitorService, 
            IBrokerService brokerService,
            IMarketplaceManifestService marketplaceManifestService,
            IValidationService validationService,
            ISecurityService securityService,
            ICacheService cacheService) 
            : base(configuration, logger, configurationService, integrationMonitorService, brokerService, marketplaceManifestService, validationService, securityService, cacheService)
        {
            this.Client = client;
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> GetProductIntegrationStatus(MarketplaceServiceMessage<RequestProductState> message)
        {
            #region [Code]
            var productErrors = new List<IntegrationError>();

            var productIntegrationInfo = new ProductIntegrationInfo(message.Data.Id, message.Data.ReferenceId, EntityStatus.Unknown, DateTimeOffset.UtcNow);

            var productResult = await this.Client.GetProduct(message.Data.ReferenceId.AsMarketplaceServiceMessage(message));

            if (productResult.IsValid)
            {
                var productErrorResult = await this.Client.GetProductErrors(message.Data.ReferenceId.AsMarketplaceServiceMessage(message));

                if (productErrorResult.IsValid)
                {
                    if(productErrorResult.Data.ErrorsCount > 0)
                    {
                        productErrors = productErrorResult.Data.ProductErrors
                            .SelectMany(c => c.ProductErrorCategories)
                            .SelectMany(c => c.Errors)
                            .Select(x => new IntegrationError
                            {
                                ErrorMessage =x.Message
                            }).ToList();
                    }
                }
            }

            productIntegrationInfo.Status = productResult.Data?.Associations.FirstOrDefault(x => x.Platform == "B2W")?.GetProductStatus(productErrors.Any()) ?? productIntegrationInfo.Status;
            productIntegrationInfo.Errors = productErrors;

            return ServiceMessage<ProductIntegrationInfo>.CreateValidResult(message.Identity, productIntegrationInfo);
            #endregion
        }

        public override async Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Accepted, serviceMessage.Data.EventDateTime );

            var product = Mapper.Map<Product>(serviceMessage.Data.Data);

            var clientResult = await this.Client.CreateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            this.HandleProductClientResult(serviceMessage.Data.ServiceOperation, clientResult, productState);

            var productLink = new ProductLink() { Skus = new List<string> { product.Sku } };

            // aguardar resposta da b2w
            //var productLinkResult = await this.Client.LinkProduct(productLink.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            //var errors = await this.Client.GetProductErrors(product.Sku.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            result.WithData(productState);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = Mapper.Map<Product>(serviceMessage.Data.Data);

            var clientResult = await this.Client.UpdateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            this.HandleProductClientResult(serviceMessage.Data.ServiceOperation, clientResult, productState);

            result.WithData(productState);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = Mapper.Map<Product>(serviceMessage.Data.Data,
                opt =>
                {
                    opt.Items[MappingContextKeys.DisabledProduct] = true;
                });

            // aguardar resposta da b2w
            //var productLink = new ProductLink() { Skus = new List<string> { product.Sku }, Type = "unlink" };
            //var productLinkResult = await this.Client.LinkProduct(productLink.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            var clientResult = await this.Client.UpdateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            this.HandleProductClientResult(serviceMessage.Data.ServiceOperation, clientResult, productState);

            result.WithData(productState);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity);
            var inventoryState = new InventoryIntegrationInfo() {
                Id = serviceMessage.Data.Data.AffectedSku.Id,
                ReferenceId = serviceMessage.Data.Data.Id,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            };

            var clientResult = new HttpMarketplaceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            if (serviceMessage.Data.Data.HasVariations)
            {
                var variation = Mapper.Map<Variation>(serviceMessage.Data.Data.AffectedSku);
                clientResult = await this.Client.UpdateVariation(variation.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }
            else
            {
                var product = Mapper.Map<Product>(serviceMessage.Data.Data);
                clientResult = await this.Client.UpdateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }

            inventoryState.Status = this.HandleProductInventoryClientResult(serviceMessage.Data.ServiceOperation, clientResult, inventoryState);

            result.WithData(inventoryState);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<PriceIntegrationInfo>(serviceMessage.Identity);
            var priceState = new PriceIntegrationInfo()
            {
                Id = serviceMessage.Data.Data.AffectedSku.Id,
                ReferenceId = serviceMessage.Data.Data.Id,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            };

            var clientResult = new HttpMarketplaceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            if (serviceMessage.Data.Data.HasVariations)
            {
                var variation = Mapper.Map<Variation>(serviceMessage.Data.Data.AffectedSku);
                clientResult = await this.Client.UpdateVariation(variation.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }
            else
            {
                var product = Mapper.Map<Product>(serviceMessage.Data.Data);
                clientResult = await this.Client.UpdateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            }

            priceState.Status = this.HandleProductPriceClientResult(serviceMessage.Data.ServiceOperation, clientResult, priceState);

            result.WithData(priceState);

            return result;
            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.B2W;
        }

        #region Handlers

        private void HandleProductClientResult(Operation operation, HttpMarketplaceMessage result, ProductIntegrationInfo productStatus)
        {
            #region [Code]

            var errors = result.Errors?.Select(x => new IntegrationError() { ErrorMessage = x.Message })?.ToList() ?? new List<IntegrationError>();

            switch (result.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    switch (operation)
                    {
                        case Operation.Insert:
                            productStatus.Errors = errors;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                        case Operation.Update:
                            productStatus.Errors = errors;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                    }
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    productStatus.Errors = errors;
                    productStatus.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                case System.Net.HttpStatusCode.Forbidden:
                    productStatus.Errors = errors;
                    productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.Accepted:
                case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                case System.Net.HttpStatusCode.NoContent:
                    switch (operation)
                    {
                        case Operation.Update:
                            productStatus.Status = EntityStatus.Accepted;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                        default:
                            productStatus.Status = EntityStatus.Sent;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                    }
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                case System.Net.HttpStatusCode.TooManyRequests:
                case System.Net.HttpStatusCode.InternalServerError:
                default:
                    productStatus.Errors = errors;
                    productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                    productStatus.Status = EntityStatus.Unknown;
                    break;
            }
            #endregion
        }

        private EntityStatus HandleProductInventoryClientResult(Operation operation, HttpMarketplaceMessage result, InventoryIntegrationInfo productStatus)
        {
            #region [Code]

            var errors = result.Errors?.Select(x => new IntegrationError() { ErrorMessage = x.Message })?.ToList() ?? new List<IntegrationError>();

            switch (result.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    switch (operation)
                    {
                        case Operation.Insert:
                            productStatus.Errors = errors;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                        case Operation.Update:
                            productStatus.Errors = errors;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                    }
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    productStatus.Errors = errors;
                    productStatus.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                case System.Net.HttpStatusCode.Forbidden:
                    productStatus.Errors = errors;
                    productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.Accepted:
                case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                case System.Net.HttpStatusCode.NoContent:
                    switch (operation)
                    {
                        case Operation.Update:
                            productStatus.Status = EntityStatus.Accepted;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                        default:
                            productStatus.Status = EntityStatus.Sent;
                            productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                            break;
                    }
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                case System.Net.HttpStatusCode.TooManyRequests:
                case System.Net.HttpStatusCode.InternalServerError:
                default:
                    productStatus.Errors = errors;
                    productStatus.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }

            return EntityStatus.Unknown;
            #endregion
        }

        private EntityStatus HandleProductPriceClientResult(Operation operation, HttpMarketplaceMessage result, PriceIntegrationInfo productStatus)
        {
            #region [Code]

            var errors = result.Errors?.Select(x => new IntegrationError() { ErrorMessage = x.Message })?.ToList() ?? new List<IntegrationError>();

            switch (result.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    switch (operation)
                    {
                        case Operation.Insert:
                            productStatus.Errors = errors;
                            break;
                        case Operation.Update:
                            productStatus.Errors = errors;
                            break;
                    }
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                case System.Net.HttpStatusCode.Unauthorized:
                case System.Net.HttpStatusCode.Forbidden:
                    return EntityStatus.Unknown;
                    break;
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                case System.Net.HttpStatusCode.Accepted:
                case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                case System.Net.HttpStatusCode.NoContent:
                    switch (operation)
                    {
                        case Operation.Update:
                            productStatus.Status = EntityStatus.Accepted;
                            break;
                        default:
                            productStatus.Status = EntityStatus.Sent;
                            break;
                    }
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                case System.Net.HttpStatusCode.TooManyRequests:
                case System.Net.HttpStatusCode.InternalServerError:
                default:
                    productStatus.Errors = errors;
                    break;
            }

            return EntityStatus.Unknown;
            #endregion
        }
        #endregion
    }
}
