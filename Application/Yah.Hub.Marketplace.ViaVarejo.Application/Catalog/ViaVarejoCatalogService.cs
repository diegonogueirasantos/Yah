using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal.Transform;
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
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Validation.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Catalog.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog;
using Product = Yah.Hub.Domain.Catalog.Product;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Catalog
{
    public class ViaVarejoCatalogService : AbstractCatalogService, IViaVarejoCatalogService
    {
        protected IViaVarejoClient Client { get; }
        public ViaVarejoCatalogService(
            IConfiguration configuration, 
            ILogger<AbstractCatalogService> logger, 
            IAccountConfigurationService configurationService, 
            IIntegrationMonitorService integrationMonitorService, 
            IBrokerService brokerService, 
            IMarketplaceManifestService marketplaceManifestService, 
            IValidationService validationService, 
            ISecurityService securityService, 
            ICacheService cacheService,
            IViaVarejoClient viaVarejoClient) 
            : base(configuration, logger, configurationService, integrationMonitorService, brokerService, marketplaceManifestService, validationService, securityService, cacheService)
        {
            Client = viaVarejoClient;
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> GetProductIntegrationStatus(MarketplaceServiceMessage<RequestProductState> message)
        {
            #region [Code]
            var productIntegrationInfo = new ProductIntegrationInfo(message.Data.Id, message.Data.ReferenceId, EntityStatus.Unknown, DateTimeOffset.UtcNow);
            var result = new ServiceMessage<ProductIntegrationInfo>(message.Identity);
            var productId = message.Data.Id;

            foreach(var sku in message.Data.Data)
            {
                var skuStatusResult = await this.Client.GetSkuStatus(new MarketplaceServiceMessage<(string productId, string skuId)>(message.Identity, message.AccountConfiguration, (productId, sku.Sku)));

                if (skuStatusResult.Data != null && skuStatusResult.IsValid)
                {
                    if (skuStatusResult.Errors.Any())
                    {
                        result.WithErrors(skuStatusResult.Errors);
                        continue;
                    }
                    else
                    {
                        result.WithError(new Error($"Erro ao tentar obter o status de integração do produto {productId} e sku {sku.Sku}.", $"Erro desconhecido, status code: {skuStatusResult.StatusCode}", ErrorType.Technical));
                        continue;
                    }
                }

                var integrationStatus = skuStatusResult.Data.Skus.Where(x => x.SellerSkuId == sku.Sku).FirstOrDefault().Status;

                if (integrationStatus.Equals(ViaVarejoProductStatus.VALIDO))
                {
                    #region [Code]
                    var validSkuResult = await this.Client.SetSkuOption(new SkuOption(OptionType.NEW_SKU.ToString(), sku.Sku, productId).AsMarketplaceServiceMessage(message));
                    
                    if (!validSkuResult.IsValid)
                    {
                        if (validSkuResult.Errors.Any())
                        {
                            productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                            {
                                Sku = sku.Sku,
                                ParentSku = productId,
                                Name = message.Data.Data.Where(x => x.Sku == sku.Sku).FirstOrDefault().Name,
                                Status = EntityStatus.Unknown,
                                Errors = validSkuResult.Errors.Select(x => new IntegrationError() { ErrorMessage = x.Message}).ToList()
                            });
                        }
                        else
                        {
                            result.WithError(new Error($"Erro ao realizar OptIn no Marketplace Via Varejo para o produto {productId} e sku {sku.Sku}.", $"erro desconhecido status code: {validSkuResult.StatusCode}", ErrorType.Technical));
                        }
                    }
                    else
                    {
                        productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                        {
                            Sku = sku.Sku,
                            ParentSku = productId,
                            Name = message.Data.Data.Where(x => x.Sku == sku.Sku).FirstOrDefault().Name,
                            Status = EntityStatus.Accepted
                        });
                    }
                    #endregion
                }
                else if(integrationStatus.Equals(ViaVarejoProductStatus.AGUARDANDO_PROCESSAMENTO)) 
                {
                    #region [Code]
                    productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                    {
                        Sku = sku.Sku,
                        ParentSku = productId,
                        Name = message.Data.Data.Where(x => x.Sku == sku.Sku).FirstOrDefault().Name,
                        Status = EntityStatus.Sent
                    });
                    #endregion
                }
                else if (integrationStatus.Equals(ViaVarejoProductStatus.FICHA_INTEGRADA))
                {
                    #region [Code]
                    productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                    {
                        Sku = sku.Sku,
                        ParentSku = productId,
                        Name = message.Data.Data.Where(x => x.Sku == sku.Sku).FirstOrDefault().Name,
                        Status = EntityStatus.Accepted
                    });
                    #endregion
                }
                else if (integrationStatus.Equals(ViaVarejoProductStatus.INVALIDO))
                {
                    #region [Code]
                    productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                    {
                        Sku = sku.Sku,
                        ParentSku = productId,
                        Name = message.Data.Data.Where(x => x.Sku == sku.Sku).FirstOrDefault().Name,
                        Status = EntityStatus.Declined,
                        Errors = skuStatusResult.Data.Skus.Where(x => x.SellerSkuId == sku.Sku).FirstOrDefault().Violation.Select(y => new IntegrationError() { ErrorMessage = $"Code: {y.Code} message: {y.message}"}).ToList()
                    });
                    #endregion
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> CreateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = Mapper.Map<Models.Catalog.Product>(serviceMessage.Data.Data);

            var clientResult = await this.Client.UpsertProduct(new ProductWrapper(new Models.Catalog.Product[] { product }).AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
            this.HandleProductClientResult(serviceMessage.Data.ServiceOperation, clientResult, productState);

            result.WithData(productState);

            return result;

            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> UpdateProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            var productState = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            foreach(var sku in serviceMessage.Data.Data.Skus)
            {
                var resultRequestStatus = await this.Client.GetSkuStatus(new MarketplaceServiceMessage<(string productId, string skuId)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (serviceMessage.Data.Data.Id, sku.Id)));

                if (resultRequestStatus.Data != null)
                {
                    if (resultRequestStatus.Data.Skus.FirstOrDefault().Status.Equals(ViaVarejoProductStatus.INVALIDO))
                    {
                        break;
                    }
                    else
                    {
                        result.WithData(productState);
                        return result;
                    }
                }
            }

            var requestProduct = await this.CreateProduct(serviceMessage);

            result.WithData(requestProduct.Data);

            return result;

            #endregion
        }

        public async override Task<ServiceMessage<ProductIntegrationInfo>> DeleteProduct(MarketplaceServiceMessage<CommandMessage<Product>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<ProductIntegrationInfo>(serviceMessage.Identity);
            var productIntegrationInfo = new ProductIntegrationInfo(serviceMessage.Data.Data.Id, serviceMessage.Data.Data.Id, EntityStatus.Unknown, serviceMessage.Data.EventDateTime);

            var product = serviceMessage.Data.Data;

            foreach (var sku in product.Skus)
            {
                var resultRequestStatus = await this.Client.GetSkuStatus(new MarketplaceServiceMessage<(string productId, string skuId)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (serviceMessage.Data.Data.Id, sku.Id)));

                if (resultRequestStatus.Data != null && resultRequestStatus.IsValid)
                {
                    if (resultRequestStatus.Data.Skus.FirstOrDefault().Status != ViaVarejoProductStatus.FICHA_INTEGRADA)
                    {
                        var requestDeleteProd = await this.Client.DeleteProduct(new MarketplaceServiceMessage<(string productId, string skuId)>(serviceMessage.Identity,serviceMessage.AccountConfiguration,(product.Id,sku.Id)));

                        if (!requestDeleteProd.IsValid)
                        {
                            if (requestDeleteProd.Errors.Any())
                            {
                                result.WithErrors(requestDeleteProd.Errors);
                            }
                            else
                            {
                                result.WithError(new Error($"Erro ao tentar inativar o produto ID {product.Id} e sku ID {sku.Id}", "", ErrorType.Technical));
                            }
                        }
                        else
                        {
                            productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                            {
                                Sku = sku.Id,
                                ParentSku = product.Id,
                                Name = sku.Name,
                                Status = EntityStatus.Closed
                            });
                        }
                    }
                    else
                    {
                        var sites = new List<SiteStatus>() 
                        { 
                            new SiteStatus() {SiteId = 2, Status = 'N' },
                            new SiteStatus() {SiteId = 3, Status = 'N' },
                            new SiteStatus() {SiteId = 4, Status = 'N' }
                        }.ToArray();

                        var updateStatusResult = await this.Client.UpdateProductStatus(new UpdateStatus(sku.Id, sites).AsMarketplaceServiceMessage(serviceMessage));

                        if (updateStatusResult.IsValid)
                        {
                            if (updateStatusResult.Errors.Any())
                            {
                                result.WithErrors(updateStatusResult.Errors);
                            }
                            else
                            {
                                result.WithError(new Error($"Erro ao tentar inativar o produto ID {product.Id} e sku ID {sku.Id}", "", ErrorType.Technical));
                            }
                        }
                        else
                        {
                            productIntegrationInfo.Skus.Add(new SkuIntegrationInfo()
                            {
                                Sku = sku.Id,
                                ParentSku = product.Id,
                                Name = sku.Name,
                                Status = EntityStatus.Closed
                            });
                        }
                    }
                }
                else
                {
                    if (resultRequestStatus.Errors.Any())
                    {
                        result.WithErrors(resultRequestStatus.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"Erro ao tentar remover o produto ID {product.Id}","",ErrorType.Technical));
                    }
                }
            }

            productIntegrationInfo.Status = EntityStatus.Closed;

            return result;

            #endregion
        }

        public async override Task<ServiceMessage<InventoryIntegrationInfo>> UpdateInventory(MarketplaceServiceMessage<CommandMessage<ProductInventory>> serviceMessage)
        {
            #region [Code]

            var result = new ServiceMessage<InventoryIntegrationInfo>(serviceMessage.Identity);
            var inventoryState = new InventoryIntegrationInfo()
            {
                Id = serviceMessage.Data.Data.AffectedSku.Id,
                ReferenceId = serviceMessage.Data.Data.Id,
                Status = EntityStatus.Unknown,
                DateTime = serviceMessage.Data.EventDateTime
            };

            var product = Mapper.Map<InventoryUpdate>(serviceMessage.Data.Data.AffectedSku);

            var clientResult = await this.Client.UpdateInventory(product.AsMarketplaceServiceMessage(serviceMessage));
            inventoryState.Status = this.HandleProductInventoryClientResult(serviceMessage.Data.ServiceOperation, clientResult, inventoryState);

            result.WithData(inventoryState);

            return result;

            #endregion
        }

        public async override Task<ServiceMessage<PriceIntegrationInfo>> UpdatePrice(MarketplaceServiceMessage<CommandMessage<ProductPrice>> serviceMessage)
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

            var product = Mapper.Map<PriceUpdate>(serviceMessage.Data.Data.AffectedSku);

            var clientResult = await this.Client.UpdatePrice(product.AsMarketplaceServiceMessage(serviceMessage));
            priceState.Status = this.HandleProductPriceClientResult(serviceMessage.Data.ServiceOperation, clientResult, priceState);

            result.WithData(priceState);

            return result;

            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.ViaVarejo;
        }

        #region [Handlers]
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
