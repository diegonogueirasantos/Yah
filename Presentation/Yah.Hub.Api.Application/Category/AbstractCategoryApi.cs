using System;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Category;

namespace Yah.Hub.Api.Application.Category
{
    public abstract class MarketplaceCategoryApi : MarketplaceControllerBase.MarketplaceControllerBase
    {
        private ICategoryService CategoryService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public MarketplaceCategoryApi(ICategoryService categoryService, ILogger logger, ISecurityService securityService)
        {
            this.CategoryService = categoryService;
            this.Logger = logger;
            this.SecurityService = securityService;
        }

        [HttpGet("GetCategory")]
        public virtual async Task<IServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>> GetCategory(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            string categoryId)
        {
            return await HandleAction(() => this.CategoryService.GetCategory(new MarketplaceServiceMessage<string>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), categoryId)));
        }

        [HttpGet("ImportCategories")]
        public virtual async Task<IServiceMessage> ImportCategories(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.CategoryService.ImportCategories(new MarketplaceServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace())));
        }

        [HttpGet("GetAttribute")]
        public virtual async Task<ServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetAttribute(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            string attributeId)
        {
            return await this.CategoryService.GetAttribute(new MarketplaceServiceMessage<string>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), GetMarketplace(), attributeId));
        }
    }
}

