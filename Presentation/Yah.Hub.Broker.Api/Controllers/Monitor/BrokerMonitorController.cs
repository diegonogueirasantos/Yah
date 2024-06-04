using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerMonitorService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Monitor;

namespace Yah.Hub.Broker.Api.Controllers.Monitor
{
    public class BrokerMonitorController : ControllerBase
    {
        private ISecurityService SecurityService;
        private IBrokerMonitorService BrokerMonitorService;
        private ILogger Logger;

        public BrokerMonitorController(ISecurityService securityService, IBrokerMonitorService brokerMonitorService, ILogger<BrokerMonitorController> logger)
        {
            SecurityService = securityService;
            BrokerMonitorService = brokerMonitorService;
            Logger = logger;
        }

        [HttpGet("ConsumeCommands")]
        public virtual async Task<ServiceMessage> HandleCommands(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerMonitorService.ConsumeCommands(new MarketplaceServiceMessage(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace));
        }

        [HttpGet("MonitorProductStatus")]
        public virtual async Task<ServiceMessage> MonitorProductStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerMonitorService.MonitorProductStatus(new MarketplaceServiceMessage(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace));
        }

        [HttpGet("GetIntegrationSummary")]
        public virtual async Task<ServiceMessage> GetIntegrationSummary(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerMonitorService.GetIntegrationSummary(new MarketplaceServiceMessage(await SecurityService.IssueVendorTenantIdentity(vendorId, tenantId), marketplace));
        }

        [HttpGet("GetIntegrationByStatus")]
        public virtual async Task<ServiceMessage> GetIntegrationByStatus(
           [FromHeader(Name = "x-VendorId")] string vendorId,
           [FromHeader(Name = "x-TenantId")] string tenantId,
           [FromHeader(Name = "x-AccountId")] string accountId,
           [FromQuery(Name = "statuses")] List<EntityStatus> statuses,
           [FromQuery(Name = "hasError")] bool hasError,
           [FromQuery(Name = "offset")] int? offset,
           [FromQuery(Name = "limit")] int? limit)
        {
            var entityStateQuery = new EntityStateQuery() { AccountId = accountId, TenantId = tenantId, VendorId = vendorId, HasErrors = hasError, Statuses = statuses, Paging = new Common.Query.Pagination(offset ?? 0, limit ?? 10) { } };
            return await this.BrokerMonitorService.GetIntegrationByStatus(entityStateQuery.AsMarketplaceServiceMessage(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), MarketplaceAlias.MercadoLivre));
        }

    }
}
