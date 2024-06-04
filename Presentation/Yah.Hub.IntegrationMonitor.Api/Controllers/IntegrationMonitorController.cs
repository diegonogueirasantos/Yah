using System;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.IntegrationMonitor.Api
{
    public class IntegrationMonitorController : ControllerBase
    {
        private readonly IIntegrationMonitorService IntegrationMonitorService;
        private readonly ISecurityService SecurityService;

        public IntegrationMonitorController(IIntegrationMonitorService integrationMonitor, ISecurityService securityService)
        {
            this.IntegrationMonitorService = integrationMonitor;
            this.SecurityService = securityService;
        }

        [HttpGet("ConsumeCommands")]
        public virtual async Task<ServiceMessage> HandleCommands()
        {
            return await this.IntegrationMonitorService.HandleCommands();
        }

        [HttpGet("MonitorProductStatus")]
        public virtual async Task<ServiceMessage> MonitorProductStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.IntegrationMonitorService.MonitorIntegrationStatus(new MarketplaceServiceMessage<(EntityType type, bool HasBatchSystem)>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace,(EntityType.Product, false)));
        }

        [HttpGet("MonitorProductBatchStatus")]
        public virtual async Task<ServiceMessage> MonitorProductBatchStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.IntegrationMonitorService.MonitorIntegrationStatus(new MarketplaceServiceMessage<(EntityType type, bool HasBatchSystem)>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (EntityType.Product, true)));
        }

        [HttpGet("MonitorPriceBatchStatus")]
        public virtual async Task<ServiceMessage> MonitorPriceBatchStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.IntegrationMonitorService.MonitorIntegrationStatus(new MarketplaceServiceMessage<(EntityType type, bool HasBatchSystem)>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (EntityType.Price, true)));
        }

        [HttpGet("MonitorInventoryBatchStatus")]
        public virtual async Task<ServiceMessage> MonitorInventoryBatchStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.IntegrationMonitorService.MonitorIntegrationStatus(new MarketplaceServiceMessage<(EntityType type, bool HasBatchSystem)>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, (EntityType.Inventory, true)));
        }

        [HttpGet("GetIntegrationSummary")]
        public virtual async Task<ServiceMessage> GetIntegrationSummary(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId)
        {
            return await this.IntegrationMonitorService.GetIntegrationSummary(new MarketplaceServiceMessage(await SecurityService.IssueVendorTenantIdentity(vendorId, tenantId), MarketplaceAlias.MercadoLivre));
        }

        [HttpGet("GetIntegrationByStatus")]
        public virtual async Task<ServiceMessage> GetIntegrationByStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromQuery(Name = "statuses")] List<EntityStatus> statuses,
           [FromQuery(Name = "id")] string id,
           [FromQuery(Name = "referenceId")] string referenceId,
           [FromQuery(Name = "hasError")] bool hasError,
           [FromQuery(Name = "offset")] int? offset,
           [FromQuery(Name = "limit")] int? limit)
        {
            var entityStateQuery = new EntityStateQuery() { AccountId = accountId, TenantId = tenantId, VendorId = vendorId, HasErrors = hasError, Statuses = statuses, Id = id, ReferenceId = referenceId, Paging = new Common.Query.Pagination(offset ?? 0, limit ?? 10) { } };
            return await this.IntegrationMonitorService.QueryAsync(entityStateQuery.AsMarketplaceServiceMessage(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), MarketplaceAlias.MercadoLivre));
        }
    }
}