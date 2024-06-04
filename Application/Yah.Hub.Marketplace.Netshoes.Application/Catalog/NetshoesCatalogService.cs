using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Application.Services.Manifest.Interface;
using AutoMapper;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Marketplace.Netshoes.Application.Client;
using Yah.Hub.Marketplace.Netshoes.Application.Models;
using Product = Yah.Hub.Marketplace.Netshoes.Application.Models.Product;
using Yah.Hub.Marketplace.Netshoes.Application.Mappings;
using Yah.Hub.Common.Extensions;
using Amazon.SimpleNotificationService.Util;
using System.Net;
using Amazon.SQS.Model;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using System;
using Price = Yah.Hub.Marketplace.Netshoes.Application.Models.Price;

namespace Yah.Hub.Marketplace.Netshoes.Application.Catalog
{
    public class NetshoesCatalogService : AbstractCatalogService, INetshoesCatalogService
    {
        private  INetshoesClient Client { get; }
        public NetshoesCatalogService(
            IConfiguration configuration,
            ILogger<AbstractCatalogService> logger,
            IAccountConfigurationService configurationService,
            IIntegrationMonitorService integrationMonitorService,
            IBrokerService brokerService,
            IMarketplaceManifestService marketplaceManifestService,
            IValidationService validationService,
            ISecurityService securityService,
            ICacheService cacheService,
            INetshoesClient client) 
            : base(configuration, logger, configurationService, integrationMonitorService, brokerService, marketplaceManifestService, validationService, securityService, cacheService)
        {
            Client = client;
        }

        #region [Methods]
        public async override Task<ServiceMessage<ProductIntegrationInfo>> GetProductIntegrationStatus(MarketplaceServiceMessage<RequestProductState> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);

            var productState = new ProductIntegrationInfo(serviceMessage.Data.Id, serviceMessage.Data.Id, EntityStatus.Unknown, DateTimeOffset.UtcNow);

            foreach(var sku in serviceMessage.Data.Data)
            {
                var productStatusResult = await this.Client.GetProduct(sku.Sku.AsMarketplaceServiceMessage(serviceMessage));

                if(productStatusResult != null 
                   && !productStatusResult.IsValid
                   && !productStatusResult.StatusCode.Equals(HttpStatusCode.NotFound))
                {
                    result.WithErrors(productStatusResult.Errors);
                }

                productState.Skus.Add(this.HandleSkuIntegrationStatus(sku, productStatusResult));
            }

            if (productState.Skus.Any(x => x.Status.Equals(EntityStatus.Accepted)))
                productState.Status = EntityStatus.Accepted;
            else if (productState.Skus.Any(x => x.Status.Equals(EntityStatus.PendingValidation)))
                productState.Status = EntityStatus.PendingValidation;
            else if (productState.Skus.Any(x => x.Status.Equals(EntityStatus.Declined)))
                productState.Status = EntityStatus.Declined;

            result.WithData(productState);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);

            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            foreach (var sku in serviceMessage.Data.Data.Skus)
            {
                var netshoesProduct = Mapper.Map<Product>(sku, opt =>
                {
                    opt.Items[MappingContextKeys.Product] = serviceMessage.Data.Data;
                });

                var productStatusResult = await this.Client.GetProduct(netshoesProduct.Sku.AsMarketplaceServiceMessage(serviceMessage));

                if (productStatusResult != null && productStatusResult.IsValid)
                {
                    continue;
                }else if (productStatusResult != null && productStatusResult.StatusCode != HttpStatusCode.NotFound)
                {
                    result.WithErrors(productStatusResult.Errors);
                    continue;
                }

                var productResult = await this.Client.CreateProduct(netshoesProduct.AsMarketplaceServiceMessage(serviceMessage));
                var skuState = this.HandleSkuClientResult(sku, productResult, serviceMessage.Data.EventDateTime);

                if (!productResult.IsValid)
                {
                    result.WithErrors(productResult.Errors);
                    continue;
                }

                var priceResult = await this.Client.CreatePrice(new MarketplaceServiceMessage<(Price Price, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Price, netshoesProduct.Sku)));
                skuState.PriceIntegrationInfo = this.HandlePriceClientResult(sku, priceResult);

                var stockResult = await this.Client.CreateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Stock, netshoesProduct.Sku)));
                skuState.InventoryIntegrationInfo = this.HandleStockClientResult(sku, stockResult);

                productState.Skus.Add(skuState);
            }

            result.WithData(productState);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);

            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var productResult = new HttpMarketplaceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            foreach (var sku in serviceMessage.Data.Data.Skus)
            {
                var netshoesProduct = Mapper.Map<Product>(sku, opt =>
                {
                    opt.Items[MappingContextKeys.Product] = serviceMessage.Data.Data;
                });

                var productStatusResult = await this.Client.GetProduct(netshoesProduct.Sku.AsMarketplaceServiceMessage(serviceMessage));

                if (productStatusResult != null && productStatusResult.IsValid)
                {
                    if (this.SkuCanBeUpdate(productStatusResult.Data.Status.Status))
                    {
                        productResult = await this.Client.UpdateProduct(netshoesProduct.AsMarketplaceServiceMessage(serviceMessage));
                        var skuState = this.HandleSkuClientResult(sku, productResult, serviceMessage.Data.EventDateTime);

                        if (!productResult.IsValid)
                        {
                            result.WithErrors(productResult.Errors);
                        }

                        var priceIntegrateState = await this.Client.GetPrice(netshoesProduct.Sku.AsMarketplaceServiceMessage(serviceMessage));

                        if (priceIntegrateState != null && priceIntegrateState.StatusCode.Equals(HttpStatusCode.NotFound))
                        {
                            var priceIntegrateResult = await this.Client.CreatePrice(new MarketplaceServiceMessage<(Price Price, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Price, netshoesProduct.Sku)));
                            skuState.PriceIntegrationInfo = this.HandlePriceClientResult(sku, priceIntegrateResult);
                        }
                        else if (priceIntegrateState != null && priceIntegrateState.IsValid)
                        {
                            var priceIntegrateResult = await this.Client.UpdatePrice(new MarketplaceServiceMessage<(Price Price, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Price, netshoesProduct.Sku)));
                            skuState.PriceIntegrationInfo = this.HandlePriceClientResult(sku, priceIntegrateResult);
                        }
                        else
                        {
                            result.WithErrors(priceIntegrateState.Errors);
                        }

                        var stockIntegrateState = await this.Client.GetStock(netshoesProduct.Sku.AsMarketplaceServiceMessage(serviceMessage));

                        if (stockIntegrateState != null && stockIntegrateState.StatusCode.Equals(HttpStatusCode.NotFound))
                        {
                            var stockIntgrateResult = await this.Client.UpdateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Stock, netshoesProduct.Sku)));
                            skuState.InventoryIntegrationInfo = this.HandleStockClientResult(sku, stockIntgrateResult);
                        }
                        else if (stockIntegrateState != null && stockIntegrateState.IsValid)
                        {
                            var stockIntgrateResult = await this.Client.CreateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Stock, netshoesProduct.Sku)));
                            skuState.InventoryIntegrationInfo = this.HandleStockClientResult(sku, stockIntgrateResult);
                        }
                        else
                        {
                            result.WithErrors(stockIntegrateState.Errors);
                        }

                        var stockResult = await this.Client.CreateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Stock, netshoesProduct.Sku)));
                        skuState.InventoryIntegrationInfo = this.HandleStockClientResult(sku, stockResult);

                        productState.Skus.Add(skuState);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (productStatusResult.StatusCode.Equals(HttpStatusCode.NotFound))
                    {

                        productResult = await this.Client.CreateProduct(netshoesProduct.AsMarketplaceServiceMessage(serviceMessage));
                        var skuState = this.HandleSkuClientResult(sku, productResult, serviceMessage.Data.EventDateTime);

                        if (!productResult.IsValid)
                        {
                            continue;
                        }

                        var priceResult = await this.Client.CreatePrice(new MarketplaceServiceMessage<(Price Price, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Price, netshoesProduct.Sku)));
                        skuState.PriceIntegrationInfo = this.HandlePriceClientResult(sku, priceResult);

                        var stockResult = await this.Client.CreateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Stock, netshoesProduct.Sku)));
                        skuState.InventoryIntegrationInfo = this.HandleStockClientResult(sku, stockResult);

                        productState.Skus.Add(skuState);
                    }
                    else
                    {
                        result.WithErrors(productStatusResult.Errors);
                    }
                }
            }

            result.WithData(productState);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Domain.Catalog.Product>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);

            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            foreach (var sku in serviceMessage.Data.Data.Skus)
            {
                var netshoesProduct = Mapper.Map<Product>(sku, opt =>
                {
                    opt.Items[MappingContextKeys.Product] = serviceMessage.Data.Data;
                });

                netshoesProduct.Stock.Available = 0; 

                var stockResult = await this.Client.UpdateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (netshoesProduct.Stock, netshoesProduct.Sku)));
            }

            productState.Status = EntityStatus.Closed;

            result.WithData(productState);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity);
            var affectedSku = serviceMessage.Data.Data.AffectedSku;

            var stock = new Stock() { Available = affectedSku.Inventory.Balance };

            var stockResult = await this.Client.UpdateStock(new MarketplaceServiceMessage<(Stock Stock, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (stock, affectedSku.Id)));
            var inventoryState = this.HandleStockClientResult(affectedSku, stockResult);

            result.WithData(inventoryState);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<PriceIntegrationInfo>(serviceMessage.Identity);
            var affectedSku = serviceMessage.Data.Data.AffectedSku;

            var price = new Price() { ListPrice = affectedSku.Price.List, SalePrice = affectedSku.Price.SalePrice ?? affectedSku.Price.Retail };

            var priceIntegrateResult = await this.Client.UpdatePrice(new MarketplaceServiceMessage<(Price Price, string Sku)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (price, affectedSku.Id)));
            var inventoryState = this.HandlePriceClientResult(affectedSku, priceIntegrateResult);

            result.WithData(inventoryState);

            return result;
            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Netshoes;
        }
        #endregion

        #region [Private]
        private bool SkuCanBeUpdate(ProductStatusEnum? status)
        {
            return !status.In(ProductStatusEnum.APPROVED, ProductStatusEnum.READY_TO_SALE, ProductStatusEnum.CATALOGING);
        }
        #endregion

        #region [Handlers]
        private SkuIntegrationInfo HandleSkuClientResult(Sku sku, HttpMarketplaceMessage message, DateTimeOffset dateTime)
        {
            #region [Code]

            var skuState = new SkuIntegrationInfo()
            {
                Name = sku.Name,
                Sku = sku.Id,
                ParentSku = sku.ProductId,
                Status = EntityStatus.Unknown,
                DateTime = dateTime,
            };

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

            return skuState;
            #endregion
        }
        
        private PriceIntegrationInfo HandlePriceClientResult(Domain.Catalog.Sku sku, HttpMarketplaceMessage message)
        {
            #region [Code]
            var priceState = new PriceIntegrationInfo()
            {
                Id = sku.Id,
                ReferenceId = sku.ProductId,
                List = sku.Price.List,
                Retail = sku.Price.Retail,
                SalePrice = sku.Price.SalePrice,
                SalePriceFrom = sku.Price.SalePriceFrom,
                SalePriceTo = sku.Price.SalePriceTo
            };

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

            return priceState;
            #endregion
        }

        private InventoryIntegrationInfo HandleStockClientResult(Domain.Catalog.Sku sku, HttpMarketplaceMessage message)
        {
            #region [Code]
            var stockState = new InventoryIntegrationInfo()
            {
                Id = sku.Id,
                ReferenceId = sku.ProductId,
                Balance = sku.Inventory.Balance,
                HandlingDays = sku.Inventory.HandlingDays
            };

            switch (message.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    stockState.IntegrationActionResult = IntegrationActionResult.NotAuthorized;
                    stockState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    stockState.Status = EntityStatus.Declined;
                    break;
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.NotFound:
                    stockState.Errors = message.Errors?.Count() > 0 ? message.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message }).ToList() : new List<IntegrationError>();
                    stockState.Status = EntityStatus.Unknown;
                    stockState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
                case HttpStatusCode.TooManyRequests:
                    stockState.Status = EntityStatus.Unknown;
                    stockState.IntegrationActionResult = IntegrationActionResult.Retry;
                    break;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    stockState.Status = EntityStatus.Accepted;
                    stockState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;

                default:
                    stockState.Status = EntityStatus.Unknown;
                    stockState.IntegrationActionResult = IntegrationActionResult.Continue;
                    break;
            }

            return stockState;
            #endregion
        }

        private SkuIntegrationInfo HandleSkuIntegrationStatus(SkuIntegrationInfo sku, HttpMarketplaceMessage<Product> message)
        {
            #region [Code]
            var skuState = new SkuIntegrationInfo()
            {
                Name = sku.Name,
                Sku = sku.Sku,
                ParentSku = sku.ParentSku,
                Status = EntityStatus.Unknown,
                DateTime = DateTimeOffset.UtcNow,
            };

            switch (message.Data.Status.Status)
            {
                case ProductStatusEnum.APPROVED:
                case ProductStatusEnum.READY_TO_SALE:
                    skuState.Status = EntityStatus.Accepted;
                    break;
                case ProductStatusEnum.CATALOGING:
                case ProductStatusEnum.RECEIVED:
                    sku.Status = EntityStatus.PendingValidation;
                    break;
                case ProductStatusEnum.CRITICIZED:
                    sku.Status = EntityStatus.Declined;
                    sku.Errors = message.Data.Status.Reviews?
                                 .Select(error => new IntegrationError()
                                 {
                                     ErrorMessage = error.Message
                                 }).ToList();
                    break;
            }

            return skuState;
            #endregion
        }
        #endregion

    }
}
