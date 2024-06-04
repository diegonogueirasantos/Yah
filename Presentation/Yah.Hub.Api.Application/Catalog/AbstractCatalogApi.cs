using System;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;

namespace Yah.Hub.Api.Application.Catalog
{
    public abstract class MarketplaceCatalogApi : MarketplaceControllerBase.MarketplaceControllerBase
    {
        private ICatalogService CatalogService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public MarketplaceCatalogApi(ICatalogService catalogService, ILogger logger, ISecurityService securityService)
        {
            this.CatalogService = catalogService;
            this.Logger = logger;
            this.SecurityService = securityService;
        }

        [HttpPost("ExecuteProductCommand")]
        public virtual async Task<IServiceMessage> ExecuteProductCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody]CommandMessage<Product> productMessage)
        {
            productMessage.Marketplace = GetMarketplace();

            return await HandleAction(() => this.CatalogService.ExecuteProductCommand(new ServiceMessage<CommandMessage<Product>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), productMessage)));
        }

        [HttpPost("ExecutePriceCommand")]
        public virtual async Task<IServiceMessage> ExecutePriceCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<ProductPrice> priceMessage)
        {
            return await HandleAction(() => this.CatalogService.ExecuteProductPriceCommand(new ServiceMessage<CommandMessage<ProductPrice>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), priceMessage)));
        }

        [HttpPost("ExecuteInventoryCommand")]
        public virtual async Task<IServiceMessage> ExecuteInventoryCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<ProductInventory> inventoryMessage)
        {
            return await HandleAction(() => this.CatalogService.ExecuteProductInventoryCommand(new ServiceMessage<CommandMessage<ProductInventory>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), inventoryMessage)));
        }

        [HttpGet("ConsumeProductCommands")]
        public virtual async Task<IServiceMessage> ConsumeProductCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.CatalogService.ConsumeProductCommand(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeProductPriceCommands")]
        public virtual async Task<IServiceMessage> ConsumeProductPriceCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.CatalogService.ConsumeProductPriceCommand(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeProductInventoryCommands")]
        public virtual async Task<IServiceMessage> ConsumeProductInventoryCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.CatalogService.ConsumeProductInventoryCommand(new ServiceMessage(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeRequestProductState")]
        public virtual async Task<IServiceMessage> MonitorProductStatus(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.CatalogService.ConsumeProductRequestState(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpPost("EnqueueProductCommand")]
        public virtual async Task<IServiceMessage> EnqueueProductCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<Product> productMessage)
        {
            return await HandleAction(() => this.CatalogService.EnqueueProductCommand(new ServiceMessage<CommandMessage<Product>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), productMessage)));
        }

        [HttpPost("EnqueueProductPriceCommand")]
        public virtual async Task<IServiceMessage> EnqueueProductPriceCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<ProductPrice> priceMessage)
        {
            return await HandleAction(() => this.CatalogService.EnqueueProductPriceCommand(new ServiceMessage<CommandMessage<ProductPrice>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), priceMessage)));
        }

        [HttpPost("EnqueueProductInventoryCommand")]
        public virtual async Task<IServiceMessage> EnqueueProductInventoryCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<ProductInventory> inventoryMessage)
        {
            return await HandleAction(() => this.CatalogService.EnqueueProductInventoryCommand(new ServiceMessage<CommandMessage<ProductInventory>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), inventoryMessage)));
        }
    }
}