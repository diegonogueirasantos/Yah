using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCatalogService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Broker.Api.Controllers.Catalog
{
    public  class BrokerCatalogController : ControllerBase
    {
        private  ISecurityService SecurityService;
        private  IBrokerCatalogService BrokerServiceApi;
        private ILogger Logger;
        public BrokerCatalogController(IBrokerCatalogService brokerServiceApi , ISecurityService securityService, ILogger<BrokerCatalogController> logger)
        {
            SecurityService = securityService;
            BrokerServiceApi = brokerServiceApi;
            Logger = logger;
        }

        #region [Product]
        /// <summary>
        /// Cria o produto/sku no marketplace
        /// </summary>
        /// <returns></returns>
        [HttpPost("catalog/product")]
        public async Task<ServiceMessage> CreateProduct(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody]Product product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(Product RequestData, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Insert)));
        }

        /// <summary>
        /// Atualiza os dados do produto/sku no marketplace
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPut("catalog/product")]
        public async Task<ServiceMessage> UpdateProduct(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] Product product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(Product RequestData, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Update)));
        }

        /// <summary>
        /// Remove o produto/sku do marketplace
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpDelete("catalog/product")]
        public async Task<ServiceMessage> DeleteProduct(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] Product product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(Product RequestData, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Delete)));
        }
        #endregion

        #region [Price]
        /// <summary>
        /// Atualiza o pre�o de uma oferta (sku) no marketplace
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="tenantId"></param>
        /// <param name="accountId"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPut("catalog/price")]
        public async Task<ServiceMessage> UpdatePrice(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] ProductPrice product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(ProductPrice Product, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Update)));
        }
        #endregion

        #region [Inventory]
        /// <summary>
        /// Atualiza o estoque de uma oferta (sku) no marketplace
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="tenantId"></param>
        /// <param name="accountId"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPut("catalog/inventory")]
        public async Task<ServiceMessage> UpdateInventory(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] ProductInventory product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(ProductInventory Product, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Update)));
        }
        #endregion

        #region [Announcement]
        /// <summary>
        /// Cria o produto/sku no marketplace
        /// </summary>
        /// <returns></returns>
        [HttpPost("catalog/announcement")]
        public async Task<ServiceMessage> CreateAnnouncement(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] Announcement product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(Announcement RequestData, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Insert)));
        }

        /// <summary>
        /// Atualiza os dados do produto/sku no marketplace
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPut("catalog/announcement")]
        public async Task<ServiceMessage> UpdateAnnouncement(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] Announcement product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(Announcement RequestData, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Update)));
        }

        /// <summary>
        /// Remove o produto/sku do marketplace
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpDelete("catalog/announcement")]
        public async Task<ServiceMessage> DeleteAnnouncement(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] Announcement product)
        {
            return await this.BrokerServiceApi.RequestAsync(new MarketplaceServiceMessage<(Announcement RequestData, Operation OperationId)>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (product, Operation.Delete)));
        }
        #endregion

    }
}