using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerOrderService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;

namespace Yah.Hub.Broker.Api.Controllers.Order
{
    public class BrokerOrderController : ControllerBase
    {
        private ISecurityService SecurityService;
        private IBrokerOrderService BrokerOrderServiceApi;
        private ILogger Logger;

        public BrokerOrderController(ISecurityService securityService, IBrokerOrderService brokerOrderService, ILogger<BrokerOrderController> logger)
        {
            SecurityService = securityService;
            BrokerOrderServiceApi = brokerOrderService;
            Logger = logger;
        }

        /// <summary>
        /// Atualiza o pedido para Faturado
        /// </summary>
        /// <returns></returns>
        [HttpGet("order/GetShipmentLabel/{orderId}")]
        public async Task<ServiceMessage<ShipmentLabel>> GetShipmentLabel(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromRoute] IOrderReference orderId)
        {
            return await this.BrokerOrderServiceApi.GetShipmentLabel(new MarketplaceServiceMessage<IOrderReference>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, orderId));
        }

        /// <summary>
        /// Atualiza o pedido para Faturado
        /// </summary>
        /// <returns></returns>
        [HttpPut("order/invoice")]
        public async Task<ServiceMessage> InvoiceOrder(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] OrderStatusInvoice statusRequest)
        {
            return await this.BrokerOrderServiceApi.UpdateOrder(new MarketplaceServiceMessage<OrderStatusInvoice>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, statusRequest));
        }

        /// <summary>
        /// Atualiza o pedido para Cancelado
        /// </summary>
        /// <returns></returns>
        [HttpPut("order/cancel")]
        public async Task<ServiceMessage> CancelOrder(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] OrderStatusCancel statusRequest)
        {
            return await this.BrokerOrderServiceApi.UpdateOrder(new MarketplaceServiceMessage<OrderStatusCancel>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, statusRequest));
        }

        /// <summary>
        /// Atualiza o pedido para Enviado
        /// </summary>
        /// <returns></returns>
        [HttpPut("order/shipment")]
        public async Task<ServiceMessage> ShipOrder(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] OrderStatusShipment statusRequest)
        {
            return await this.BrokerOrderServiceApi.UpdateOrder(new MarketplaceServiceMessage<OrderStatusShipment>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, statusRequest));
        }

        /// <summary>
        /// Atualiza o pedido para Enviado
        /// </summary>
        /// <returns></returns>
        [HttpPut("order/shipmentException")]
        public async Task<ServiceMessage> ShipExceptionOrder(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] OrderStatusShipException statusRequest)
        {
            return await this.BrokerOrderServiceApi.UpdateOrder(new MarketplaceServiceMessage<OrderStatusShipException>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, statusRequest));
        }

        /// <summary>
        /// Atualiza o pedido para Entregue
        /// </summary>
        /// <returns></returns>
        [HttpPut("order/delivery")]
        public async Task<ServiceMessage> DeliveryOrder(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace,
            [FromBody] OrderStatusDelivered statusRequest)
        {
            return await this.BrokerOrderServiceApi.UpdateOrder(new MarketplaceServiceMessage<OrderStatusDelivered>(await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace, statusRequest));
        }
    }
}
