using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCatalogService;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCategoryService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Category;

namespace Yah.Hub.Broker.Api.Controllers.Category
{
    public class BrokerCategoryController : ControllerBase
    {
        private ISecurityService SecurityService;
        private IBrokerCategoryService BrokerCategoryService;
        private ILogger Logger;

        public BrokerCategoryController(ISecurityService securityService, IBrokerCategoryService brokerCategoryService, ILogger<BrokerCategoryController> logger)
        {
            this.SecurityService = securityService;
            this.BrokerCategoryService = brokerCategoryService;
            Logger = logger;
        }

        /// <summary>
        /// Controller responsável por obter uma categoria do marketplace
        /// </summary>
        /// <returns></returns>
        [HttpPost("category/{id}")]
        public async Task<ServiceMessage<List<MarketplaceCategory>>> GetCategoryById(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromRoute] string id)
        {
            return await this.BrokerCategoryService.GetCategory(new MarketplaceServiceMessage<string>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, id));
        }

        /// <summary>
        /// Controller responsável por obter todas as categorias de um marketplace
        /// </summary>
        /// <returns></returns>
        [HttpPost("category/")]
        public async Task<ServiceMessage<List<MarketplaceCategory>>> GetCategories(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerCategoryService.GetCategories(new MarketplaceServiceMessage(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace));
        }

        [HttpGet("attribute")]
        public virtual async Task<ServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetAttribute(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerCategoryService.GetAttribute(new MarketplaceServiceMessage<string>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, null));
        }

        [HttpGet("attribute/{id}")]
        public virtual async Task<ServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetAttribute(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            string id)
        {
            return await this.BrokerCategoryService.GetAttribute(new MarketplaceServiceMessage<string>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, id));
        }
    }
}
