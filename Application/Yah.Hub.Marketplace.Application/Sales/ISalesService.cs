using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;
using System;
namespace Yah.Hub.Marketplace.Application.Sales
{
    public interface ISalesService
    {
        Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message);
        Task<ServiceMessage> RetrieveOrdersFromMarketplaceAsync(ServiceMessage message, bool isScan);
        Task<ServiceMessage> ChangeOrderStatusCancelAsync(ServiceMessage<OrderStatusCancel> message);
        Task<ServiceMessage> ChangeOrderStatusInvoiceAsync(ServiceMessage<OrderStatusInvoice> message);
        Task<ServiceMessage> ChangeOrderStatusShipAsync(ServiceMessage<OrderStatusShipment> message);
        Task<ServiceMessage> ChangeOrderStatusDeliveryAsync(ServiceMessage<OrderStatusDelivered> message);
        Task<ServiceMessage> ChangeOrderStatusShipExceptionAsync(ServiceMessage<OrderStatusShipException> message);
        Task<ServiceMessage<ShipmentLabel>> GetMarketplaceShipmentLabelAsync(MarketplaceServiceMessage<string> message);
        Task<ServiceMessage<ShipmentLabel>> GetMarketplaceShipmentLabelAsync(MarketplaceServiceMessage<IOrderReference> []message);
        Task<ServiceMessage<Order>> GetOrderAsync(ServiceMessage<string> message);
        Task<ServiceMessage> SendOrdersToIntegrationAsync(ServiceMessage message);
        Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Order> message);
    }
}

