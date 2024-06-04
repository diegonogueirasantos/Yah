using Microsoft.AspNetCore.Mvc;
using Nest;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Common.Enums;

namespace Yah.Hub.Api.Application.Catalog
{
    public abstract class MarketplaceBatchCatalogApi : MarketplaceCatalogApi
    {
        private ICatalogService CatalogService;
        private IBatchCatalogService BatchCatalogService;
        private ILogger Logger;
        private ISecurityService SecurityService;
        public MarketplaceBatchCatalogApi(ICatalogService catalogService, ILogger<MarketplaceBatchCatalogApi> logger, ISecurityService securityService, IBatchCatalogService batchCatalogService) : base(catalogService, logger, securityService)
        {
            Logger = logger;
            SecurityService = securityService;
            BatchCatalogService = batchCatalogService;
            CatalogService = catalogService;
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            throw new NotImplementedException();
        }

        [HttpPost("ProcessInsertProductBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessInsertProductBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(), 
                    (BatchType.PRODUCT, Operation.Insert))));
        }

        [HttpPost("ProcessUpdateProductBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessUpdateProductBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(),
                    (BatchType.PRODUCT, Operation.Update))));
        }

        [HttpPost("ProcessDeleteProductBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessDeleteProductBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(),
                    (BatchType.PRODUCT, Operation.Delete))));
        }

        [HttpPost("ProcessInsertPriceBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessInsertPriceBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(),
                    (BatchType.PRICE, Operation.Insert))));
        }

        [HttpPost("ProcessUpdatePriceBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessUpdatePriceBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(),
                    (BatchType.PRICE, Operation.Update))));
        }

        [HttpPost("ProcessInsertInventoryBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessInsertInventoryBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(),
                    (BatchType.INVENTORY, Operation.Insert))));
        }

        [HttpPost("ProcessUpdateInventoryBatchAsync")]
        public virtual async Task<IServiceMessage> ProcessUpdateInventoryBatchAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.BatchCatalogService.ProcessBatchAsync(
                new ServiceMessage<(Domain.BatchItem.BatchType BatchType, Common.Enums.Operation CommandType)>
                    (SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId)
                    .GetAwaiter()
                    .GetResult(),
                    (BatchType.INVENTORY, Operation.Update))));
        }
    }
}
