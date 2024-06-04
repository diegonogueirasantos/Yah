using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Common.Query;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Enums;

namespace Yah.Hub.Api.Application.Catalog
{
    public abstract class MarketplaceAnnouncementApi : MarketplaceControllerBase.MarketplaceControllerBase
    {
        private IAnnouncementService AnnouncementService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public MarketplaceAnnouncementApi(IAnnouncementService announcementService, ILogger logger, ISecurityService securityService)
        {
            this.AnnouncementService = announcementService;
            this.Logger = logger;
            this.SecurityService = securityService;
        }

        [HttpPost("ExecuteProductCommand")]
        public virtual async Task<IServiceMessage> ExecuteProductCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody]CommandMessage<Product> productMessage)
        {
            if (productMessage == null || productMessage.Data == null)
                return ServiceMessage.CreateInvalidResult(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), new Error("CommandMessage or Data could not be null","", ErrorType.Technical));

            productMessage.IsAnnouncement = true;
            return await HandleAction(() => this.AnnouncementService.ExecuteProductCommand(new ServiceMessage<CommandMessage<Product>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), productMessage)));
        }

        [HttpPost("EnqueueProductCommand")]
        public virtual async Task<IServiceMessage> EnqueueProductCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<Product> productMessage)
        {
            productMessage.IsAnnouncement = true;
            return await HandleAction(() => this.AnnouncementService.EnqueueProductCommand(new ServiceMessage<CommandMessage<Product>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), productMessage)));
        }

        [HttpPost("ExecuteProductPriceCommand")]
        public virtual async Task<IServiceMessage> ExecuteProductPriceCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody]CommandMessage<ProductPrice> priceCommand)
        {
            priceCommand.IsAnnouncement = true;
            return await HandleAction(() => this.AnnouncementService.ExecuteProductPriceCommand(new ServiceMessage<CommandMessage<ProductPrice>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), priceCommand)));
        }

        [HttpPost("EnqueueProductPriceCommand")]
        public virtual async Task<IServiceMessage> EnqueueProductPriceCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<ProductPrice> priceCommand)
        {
            priceCommand.IsAnnouncement = true;
            return await HandleAction(() => this.AnnouncementService.EnqueueProductPriceCommand(new ServiceMessage<CommandMessage<ProductPrice>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), priceCommand)));
        }

        [HttpPost("ExecuteProductInventoryCommand")]
        public virtual async Task<IServiceMessage> ExecuteProductInventoryCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody]CommandMessage<ProductInventory> inventoryCommand)
        {
            inventoryCommand.IsAnnouncement = true;
            return await HandleAction(() => this.AnnouncementService.ExecuteProductInventoryCommand(new ServiceMessage<CommandMessage<ProductInventory>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), inventoryCommand)));
        }

        [HttpPost("EnqueueProductInventoryCommand")]
        public virtual async Task<IServiceMessage> EnqueueProductInventoryCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<ProductInventory> inventoryCommand)
        {
            inventoryCommand.IsAnnouncement = true;
            return await HandleAction(() => this.AnnouncementService.EnqueueProductInventoryCommand(new ServiceMessage<CommandMessage<ProductInventory>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), inventoryCommand)));
        }

        [HttpPost("CreateAnnouncement")]
        public virtual async Task<IServiceMessage<Announcement>> CreateAnnouncement(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] Announcement announcement)
        {
            return await HandleAction(() => this.AnnouncementService.CreateAnnouncement(new MarketplaceServiceMessage<Announcement>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), announcement)));
        }

        [HttpPut("UpdateAnnouncement")]
        public virtual async Task<IServiceMessage> UpdateAnnouncement(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] Announcement announcement)
        {
            return await HandleAction(() => this.AnnouncementService.UpdateAnnouncement(new MarketplaceServiceMessage<Announcement>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), announcement)));
        }

        [HttpDelete("DeleteAnnouncement")]
        public virtual async Task<IServiceMessage> DeleteAnnouncement(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] string announcementId)
        {
            return await HandleAction(() => this.AnnouncementService.DeleteAnnouncement(new MarketplaceServiceMessage<string>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), announcementId)));
        }

        [HttpPut("ChangeAnnouncementState")]
        public virtual async Task<IServiceMessage> ChangeAnnouncementState(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] ChangeAnnouncementState announcementState )
        {
            return await HandleAction(() => this.AnnouncementService.ChangeAnnouncementState(new MarketplaceServiceMessage<(string announcementId, AnnouncementState state)>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), (announcementState.AnnouncementId, announcementState.State))));
        }

        [HttpGet("GetAnnouncementById")]
        public async Task<IServiceMessage> GetAnnouncementById(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromQuery] string Id)
        {
            return await HandleAction(() => this.AnnouncementService.GetAnnouncementById(new MarketplaceServiceMessage<string>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), Id)));
        }

        [HttpPost("ExecuteAnnouncementCommand")]
        public async Task<IServiceMessage> ExecuteAnnouncementCommand(
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<Announcement> message)
        {
            return await HandleAction(() => this.AnnouncementService.ExecuteAnnouncementCommand(new ServiceMessage<CommandMessage<Announcement>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), message)));
        }

        [HttpPost("ExecuteAnnouncementPriceCommand")]
        public async Task<IServiceMessage> ExecuteAnnouncementPriceCommand(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<AnnouncementPrice> message)
        {
            return await HandleAction(() => this.AnnouncementService.ExecuteAnnouncementPriceCommand(new ServiceMessage<CommandMessage<AnnouncementPrice>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), message)));
        }

        [HttpPost("ExecuteAnnouncementInventoryCommand")]
        public async Task<IServiceMessage> ExecuteAnnouncementInventoryCommand(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<AnnouncementInventory> message)
        {
            return await HandleAction(() => this.AnnouncementService.ExecuteAnnouncementInventoryCommand(new ServiceMessage<CommandMessage<AnnouncementInventory>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), message)));
        }

        [HttpGet("ConsumeAnnouncementCommands")]
        public virtual async Task<IServiceMessage> ConsumeAnnouncementCommand(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ConsumeAnnouncementCommand(new ServiceMessage(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeAnnouncementPriceCommands")]
        public virtual async Task<IServiceMessage> ConsumeAnnouncementPriceCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ConsumeAnnouncementPriceCommand(new ServiceMessage(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeAnnouncementInventoryCommands")]
        public virtual async Task<IServiceMessage> ConsumeAnnouncementInventoryCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ConsumeAnnouncementInventoryCommand(new ServiceMessage(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult())));
        }


        [HttpGet("ConsumeProductCommands")]
        public virtual async Task<IServiceMessage> ConsumeProductCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ConsumeProductCommand(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeProductPriceCommands")]
        public virtual async Task<IServiceMessage> ConsumeProductPriceCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ConsumeProductPriceCommand(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpGet("ConsumeProductInventoryCommands")]
        public virtual async Task<IServiceMessage> ConsumeProductInventoryCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ConsumeProductInventoryCommand(new ServiceMessage(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult())));
        }
        [HttpPost("EnqueueAnnouncementCommand")]
        public virtual async Task<IServiceMessage> EnqueueAnnouncementCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<Announcement> productMessage)
        {
            return await HandleAction(() => this.AnnouncementService.EnqueueAnnouncementCommand(new ServiceMessage<CommandMessage<Announcement>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), productMessage)));
        }

        [HttpPost("EnqueueAnnouncementPriceCommand")]
        public virtual async Task<IServiceMessage> EnqueueAnnouncementPriceCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<AnnouncementPrice> priceMessage)
        {
            return await HandleAction(() => this.AnnouncementService.EnqueueAnnouncementPriceCommand(new ServiceMessage<CommandMessage<AnnouncementPrice>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), priceMessage)));
        }

        [HttpPost("EnqueueAnnouncementInventoryCommand")]
        public virtual async Task<IServiceMessage> EnqueueAnnouncementInventoryCommand(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] CommandMessage<AnnouncementInventory> inventoryMessage)
        {
            return await HandleAction(() => this.AnnouncementService.EnqueueAnnouncementInventoryCommand(new ServiceMessage<CommandMessage<AnnouncementInventory>>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), inventoryMessage)));
        }

        [HttpPost("ReplicateAllAnnouncement")]
        public async Task<IServiceMessage> ReplicateAllAnnouncements(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AnnouncementService.ReplicateAllAnnouncement(new MarketplaceServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace())));
        }

        [HttpPost("ReplicateAnnouncementById")]
        public async Task<IServiceMessage> ReplicateAnnouncementById(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromQuery] string Id)
        {
            return await HandleAction(() => this.AnnouncementService.ReplicateAnnouncementById(new MarketplaceServiceMessage<string>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), Id)));
        }

        [HttpPost("ReplicateAnnouncement")]
        public async Task<IServiceMessage> ReplicateAnnouncements(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] Announcement announcement)
        {
            return await HandleAction(() => this.AnnouncementService.ReplicateAnnouncement(new MarketplaceServiceMessage<Announcement>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), announcement)));
        }

        [HttpGet("SearchAnnouncement")]
        public async Task<IServiceMessage> SearchAnnouncement(
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-VendorID")] string vendorId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromQuery] string title,
            [FromQuery] string filterAccountId,
            [FromQuery] string marketplaceId,
            [FromQuery] string announcementId,
            [FromQuery] string productId,
            [FromQuery] string category,
            [FromQuery] EntityStatus? status,
            [FromQuery] int offset,
            [FromQuery] int limit)
        {
            var filter = new AnnouncementQuery()
            {
                Title = title,
                MarketplaceId = marketplaceId,
                AccountId = filterAccountId,
                Paging = new Pagination(offset, limit),
                TenantId = tenantId,
                VendorId = vendorId,
                AnnouncementId = announcementId,
                ProductId = productId,
                Category = category,
                Status = status
            };

            return await HandleAction(() => this.AnnouncementService.QueryAsync(new MarketplaceServiceMessage<AnnouncementQuery>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), filter)));
        }
    }
}