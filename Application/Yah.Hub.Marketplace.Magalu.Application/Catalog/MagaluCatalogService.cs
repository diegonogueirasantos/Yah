using AutoMapper;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Magalu.Application.Mappings;
using Yah.Hub.Marketplace.Magalu.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Models;
using System.Net;
using Category = Yah.Hub.Marketplace.Magalu.Application.Models.Category;
using Price = Yah.Hub.Marketplace.Magalu.Application.Models.Price;
using Product = Yah.Hub.Marketplace.Magalu.Application.Models.Product;
using Sku = Yah.Hub.Marketplace.Magalu.Application.Models.Sku;
using Amazon.SimpleNotificationService.Util;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Amazon.SQS.Model;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Magalu.Application.Catalog.Interface;
using Yah.Hub.Common.Services.CacheService;

namespace Yah.Hub.Marketplace.Magalu.Application.Catalog
{
    public class MagaluCatalogService : AbstractCatalogService, IMagaluCatalogService
    {
        private IMagaluClient Client { get; }

        public MagaluCatalogService(
            IConfiguration configuration,
            ILogger<AbstractCatalogService> logger,
            IAccountConfigurationService configurationService,
            IMagaluClient client,
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
            var productIntegrationInfo = new ProductIntegrationInfo(message.Data.Id, message.Data.ReferenceId, EntityStatus.Unknown, DateTimeOffset.UtcNow);

            var productResult = await this.Client.GetProduct(message.Data.ReferenceId.AsMarketplaceServiceMessage(message));

            if (!productResult.IsValid)
            {
                return ServiceMessage<ProductIntegrationInfo>.CreateInvalidResult(message.Identity, new Error($"Erro ao tentar obter o status de integração do produto {productIntegrationInfo.ReferenceId}","",ErrorType.Technical),productIntegrationInfo);
            }

            productIntegrationInfo.Status = (productResult.Data.Active ?? false) ? EntityStatus.Accepted : productIntegrationInfo.Status;

            for(var i = 0; i < productIntegrationInfo.Skus.Count(); i++)
            {
                var status = EntityStatus.Unknown;
                var skuResult = await this.Client.GetSku(productIntegrationInfo.Skus[i].Sku.AsMarketplaceServiceMessage(message));

                if (skuResult.IsValid)
                    status = EntityStatus.Accepted;

                productIntegrationInfo.Skus[i].Status = status;
            }

            return ServiceMessage<ProductIntegrationInfo>.CreateValidResult(message.Identity, productIntegrationInfo);
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = Mapper.Map<Product>(serviceMessage.Data.Data);

            var Skus = Mapper.Map<List<Sku>>(serviceMessage.Data.Data, opt =>
            {
                opt.Items[MappingContextKeys.DisabledProduct] = false;
            });

            var productResult = await this.Client.CreateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            this.HandleProductClientResult(productState, productResult);
            
            if (!productResult.IsValid)
            {
                var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
                result.WithErrors(productResult.Errors);
                result.WithData(productState);

                return result;
            }

            foreach (var sku in Skus)
            {
                var skuState = new SkuIntegrationInfo()
                {
                    Name = sku.Name,
                    Sku = sku.IdSku,
                    ParentSku = sku.IdProduct,
                    Status = EntityStatus.Unknown,
                    DateTime = serviceMessage.Data.EventDateTime,
                };

                var skuResult = await this.Client.CreateSku(sku.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

               this.HandleSkuClientResult(skuState, productResult);

                productState.Skus.Add(skuState);
            }

            return new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity, productState);
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = Mapper.Map<Product>(serviceMessage.Data.Data);

            var Skus = Mapper.Map<List<Sku>>(serviceMessage.Data.Data, opt =>
            {
                opt.Items[MappingContextKeys.DisabledProduct] = false;
            });

            var productResult = await this.Client.UpdateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            this.HandleProductClientResult(productState, productResult);

            if (!productResult.IsValid)
            {
                var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
                result.WithErrors(productResult.Errors);

                return result;
            }

            foreach (var sku in Skus)
            {
                var skuState = new SkuIntegrationInfo()
                {
                    Name = sku.Name,
                    Sku = sku.IdSku,
                    ParentSku = sku.IdProduct,
                    Status = EntityStatus.Unknown,
                    DateTime = serviceMessage.Data.EventDateTime,
                };

                var skuResult = await this.Client.UpdateSku(sku.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                this.HandleSkuClientResult(skuState, productResult);

                productState.Skus.Add(skuState);
            }

            return new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity, productState);
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = Mapper.Map<Product>(serviceMessage.Data.Data, opt => 
            {
                opt.Items[MappingContextKeys.DisabledProduct] = true;
            });

            var Skus = Mapper.Map<List<Sku>>(serviceMessage.Data.Data.Skus, opt =>
            {
                opt.Items[MappingContextKeys.DisabledProduct] = true;
            });

            var productResult = await this.Client.UpdateProduct(product.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            this.HandleProductClientResult(productState, productResult);

            if (!productResult.IsValid)
            {
                var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
                result.WithErrors(productResult.Errors);

                return result;
            }

            foreach (var sku in Skus)
            {
                var skuState = new SkuIntegrationInfo()
                {
                    Name = sku.Name,
                    Sku = sku.IdSku,
                    ParentSku = sku.IdProduct,
                    Status = EntityStatus.Unknown,
                    DateTime = serviceMessage.Data.EventDateTime,
                };

                var skuResult = await this.Client.UpdateSku(sku.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                this.HandleSkuClientResult(skuState, productResult);

                productState.Skus.Add(skuState);
            }

            return new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity, productState);
            #endregion
        }

        public async override Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            #region [Code]

            var sku = serviceMessage.Data.Data.AffectedSku;

            var priceState = new PriceIntegrationInfo()
            {
                Id = sku.Id,
                ReferenceId =  sku.ProductId,
                List = sku.Price.List,
                Retail = sku.Price.Retail,
                SalePrice = sku.Price.SalePrice,
                SalePriceTo = sku.Price.SalePriceTo,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            };

            var skus = new List<Price>();

            skus.Add(Mapper.Map<Price>(serviceMessage.Data.Data));

            var clientResult = await this.Client.UpdatePrice(skus.ToArray().AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            this.HandlePriceClientResult(priceState, clientResult);

            if (!clientResult.IsValid)
            {
                return ServiceMessage<PriceIntegrationInfo>.CreateInvalidResult(serviceMessage.Identity, clientResult.Errors, priceState);
            }

            return ServiceMessage<PriceIntegrationInfo>.CreateValidResult(serviceMessage.Identity,priceState);
            #endregion
        }

        public async override Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            #region [Code]

            var sku = serviceMessage.Data.Data.AffectedSku;

            var inventoryState = new InventoryIntegrationInfo()
            {
                Id = sku.Id,
                ReferenceId = sku.ProductId,
                Balance = sku.Inventory.Balance,
                HandlingDays = sku.Inventory.HandlingDays,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            };

            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity);

            var skus = new List<Stock>();

            skus.Add(Mapper.Map<Stock>(serviceMessage.Data.Data));

            var clientResult = await this.Client.UpdateStock(skus.ToArray().AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            this.HandleInventoryClientResult(inventoryState,clientResult);

            if (!clientResult.IsValid)
            {
                return ServiceMessage<InventoryIntegrationInfo>.CreateInvalidResult(serviceMessage.Identity, clientResult.Errors, inventoryState);
            }

            return ServiceMessage<InventoryIntegrationInfo>.CreateValidResult(serviceMessage.Identity, inventoryState);
            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Magalu;
        }

        #region [Private]

        private void HandleProductClientResult(ProductIntegrationInfo productState, HttpMarketplaceMessage message)
        {
            #region [Code]
            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    productState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    productState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    productState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    productState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message}).ToList() : new List<IntegrationError>();
                    productState.Status = EntityStatus.Unknown;
                    productState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    productState.Status = EntityStatus.Unknown;
                    productState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    productState.Status = EntityStatus.Accepted;
                    productState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    productState.Status = EntityStatus.Unknown;
                    productState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }
            #endregion
        }

        private void HandleSkuClientResult(SkuIntegrationInfo skuState, HttpMarketplaceMessage message)
        {
            #region [Code]
            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    skuState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    skuState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    skuState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    skuState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    skuState.Status = EntityStatus.Unknown;
                    skuState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    skuState.Status = EntityStatus.Unknown;
                    skuState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    skuState.Status = EntityStatus.Accepted;
                    skuState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    skuState.Status = EntityStatus.Unknown;
                    skuState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }
            #endregion
        }

        private void HandlePriceClientResult(PriceIntegrationInfo priceState, HttpMarketplaceMessage message)
        {
            #region [Code]
            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    priceState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    priceState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    priceState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    priceState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    priceState.Status = EntityStatus.Unknown;
                    priceState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    priceState.Status = EntityStatus.Unknown;
                    priceState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    priceState.Status = EntityStatus.Accepted;
                    priceState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    priceState.Status = EntityStatus.Unknown;
                    priceState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }
            #endregion
        }

        private void HandleInventoryClientResult(InventoryIntegrationInfo inventoryState, HttpMarketplaceMessage message)
        {
            #region [Code]
            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    inventoryState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    inventoryState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    inventoryState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    inventoryState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    inventoryState.Status = EntityStatus.Unknown;
                    inventoryState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    inventoryState.Status = EntityStatus.Unknown;
                    inventoryState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    inventoryState.Status = EntityStatus.Accepted;
                    inventoryState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    inventoryState.Status = EntityStatus.Unknown;
                    inventoryState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }
            #endregion
        }

        #endregion
    }
}
