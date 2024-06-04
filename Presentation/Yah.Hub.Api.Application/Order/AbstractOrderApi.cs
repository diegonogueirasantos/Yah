using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;
using Yah.Hub.Marketplace.Application.Sales;

namespace Yah.Hub.Api.Application.Order
{
    public abstract class MarketplaceOrderApi : MarketplaceControllerBase.MarketplaceControllerBase
    {
        private ISalesService SalesService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public MarketplaceOrderApi(ISalesService salesService, ILogger logger, ISecurityService securityService)
        {
            SalesService = salesService;
            Logger = logger;
            SecurityService = securityService;
        }

        [HttpGet("RetrieveOrdersFromMarketplaceAsync")]
        public virtual async Task<IServiceMessage> RetrieveOrdersFromMarketplaceAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            bool isScan)
        {
            return await HandleAction(() => SalesService.RetrieveOrdersFromMarketplaceAsync(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult()), isScan));
        }

        [HttpGet("ConsumeOrderCommandsAsync")]
        public virtual async Task<IServiceMessage> ConsumeOrderCommandsAsync()
        {
            return await HandleAction(() => SalesService.SendOrdersToIntegrationAsync(new ServiceMessage(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult())));
        }

        [HttpGet("GetShipmentLabelAsync")]
        public virtual async Task<IServiceMessage<ShipmentLabel>> GetShipmentLabelAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            string orderId)
        {
            return await HandleAction(() => SalesService.GetMarketplaceShipmentLabelAsync(new MarketplaceServiceMessage<string>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), GetMarketplace(), orderId)));
        }

        [HttpGet("GetOrder")]
        public virtual async Task<IServiceMessage<Domain.Order.Order>> GetOrder(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            string orderId)
        {
            return await HandleAction(() => SalesService.GetOrderAsync(new ServiceMessage<string>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), orderId)));
        }

        [HttpPut("TryCancelOrderAsync")]
        public virtual async Task<IServiceMessage> TryCancelOrderAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] OrderStatusCancel statusRequest)
        {
            return await HandleAction(() => SalesService.ChangeOrderStatusCancelAsync(new ServiceMessage<OrderStatusCancel>( SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), statusRequest)));
        }

        [HttpPut("TryInvoiceOrderAsync")]
        public virtual async Task<IServiceMessage> TryInvoiceOrderAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] OrderStatusInvoice statusRequest)
        {
            return await HandleAction(() => SalesService.ChangeOrderStatusInvoiceAsync(new ServiceMessage<OrderStatusInvoice>( SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), statusRequest)));
        }

        [HttpPut("TryShipOrderAsync")]
        public virtual async Task<IServiceMessage> TryShipOrderAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] OrderStatusShipment statusRequest)
        {
            return await HandleAction(() => SalesService.ChangeOrderStatusShipAsync(new ServiceMessage<OrderStatusShipment>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), statusRequest)));
        }

        [HttpPut("TryDeliveryOrderAsync")]
        public virtual async Task<IServiceMessage> TryDeliveryOrderAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] OrderStatusDelivered statusRequest)
        {
            return await HandleAction(() => SalesService.ChangeOrderStatusDeliveryAsync(new ServiceMessage<OrderStatusDelivered>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), statusRequest)));
        }
        
        [HttpPut("TryShipExceptionOrderAsync")]
        public virtual async Task<IServiceMessage> TryShipExceptionOrderAsync(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromBody] OrderStatusShipException statusRequest)
        {
            return await HandleAction(() => SalesService.ChangeOrderStatusShipExceptionAsync(new ServiceMessage<OrderStatusShipException>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), statusRequest)));
        }
    }
}