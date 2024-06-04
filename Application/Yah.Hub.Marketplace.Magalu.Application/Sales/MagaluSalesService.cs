using AutoMapper;
using Yah.Hub.Application.Clients.ExternalClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.OrderService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.OrderRequest;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.Magalu.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Models.Order;

namespace Yah.Hub.Marketplace.Magalu.Application.Sales
{
    public class MagaluSalesService : AbstractSalesService, ISalesService
    {
        private readonly IMagaluClient Client;
        private readonly IMapper Mapper;

        public MagaluSalesService(
            IConfiguration configuration,
            ILogger<AbstractSalesService> logger,
            IAccountConfigurationService configurationService,
            ICacheService cacheService, IOrderService orderService,
            IMagaluClient client,
            IMapper mapper,
            IBrokerService brokerService,
            IERPClient eRPClient) 
            : base(configuration, logger, configurationService, cacheService, orderService, brokerService, eRPClient)
        {
            this.Client = client;
            this.Mapper = mapper;
        }

        public async override Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Order> message)
        {
            #region [Code]
            
            var queueItems = new List<OrderQueueItemId>();

            if(int.TryParse(message.Data.MarketplaceOrderQueueId, out int id))
            {
                queueItems.Add(new OrderQueueItemId(id));
            }
            
            var dequeueResult = await this.Client.DequeueOrders(queueItems.ToArray().AsMarketplaceServiceMessage(message.Identity,message.AccountConfiguration));

            if (!dequeueResult.IsValid)
            {
                return ServiceMessage.CreateInvalidResult(message.Identity, dequeueResult.Errors);
            }

            return ServiceMessage.CreateValidResult(message.Identity);
            #endregion
        }

        public async override Task<ServiceMessage<List<Order>>> GetOrdersToIntegrateFromMarketplaceAsync(MarketplaceServiceMessage<DateRange> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<Order>>(message.Identity);
            var orders = new List<Order>();

            var errors = new List<Error>();
            foreach(var status in new[] { "NEW", "APPROVED", "CANCELED" })
            {
                var orderQueueResult = await this.Client.GetOrdersFromQueue(status.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if (!orderQueueResult.IsValid)
                {
                    errors.AddRange(orderQueueResult.Errors);
                }

                if (orderQueueResult?.Data?.Total > 0)
                {
                    foreach(var orderQueue in orderQueueResult?.Data?.Orders)
                    {
                        IOrderReference orderId = new OrderReference(orderQueue.IdOrder);

                        var orderResult = await this.TryGetOrderFromMarketplaceAsync(orderId.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                        if(orderResult == null || !orderResult.IsValid)
                        {
                            continue;
                        }

                        if (orderResult.Data?.Order != null)
                            orderResult.Data.Order.MarketplaceOrderQueueId = Convert.ToString(orderQueue.Id);

                        orders.Add(orderResult.Data.Order);
                    }
                }
            }

            if(!orders.Any() || errors.Any())
            {
                return ServiceMessage<List<Order>>.CreateInvalidResult(message.Identity, errors, null);
            }

            return ServiceMessage<List<Order>>.CreateValidResult(message.Identity, orders);
            #endregion
        }

        public async override Task<ServiceMessage<ShipmentLabel>> GetShipmentLabelAsync(MarketplaceServiceMessage<string> message)
        {
            #region [Code]
            var orderId = new List<string>() { message.Data };

            var label = new MagaluShipmentLabelRequest() 
            {
                OrderIds = orderId.ToArray()
            };

            var labelResult = await this.Client.GetShipmentLabel(label.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!labelResult.IsValid)
            {
                return ServiceMessage<ShipmentLabel>.CreateInvalidResult(message.Identity, labelResult.Errors, null);
            }

            var result = new ShipmentLabel() { URL = labelResult.Data[0].Url };

            return ServiceMessage<ShipmentLabel>.CreateValidResult(message.Identity, result);
            #endregion
        }

        public async override Task<ServiceMessage> TryCancelOrderAsync(MarketplaceServiceMessage<(OrderStatusCancel status, Order order)> message)
        {
            #region [Code]
            return ServiceMessage.CreateInvalidResult(message.Identity, new Error($"Não é possível cancelar pedidos no marketplace {MarketplaceAlias.Magalu}", "", ErrorType.Business));
            #endregion
        }

        public async override Task<ServiceMessage> TryDeliveryOrderAsync(MarketplaceServiceMessage<(OrderStatusDelivered status, Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var delivery = this.Mapper.Map<MagaluDeliveryOrder>(message.Data.status);

            var deliveryResult = await this.Client.DeliveryOrder(delivery.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!deliveryResult.IsValid)
                result.WithErrors(deliveryResult.Errors);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]
            var orderResult = await this.Client.GetOrder(message.Data.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!orderResult.IsValid)
            {
                return ServiceMessage<OrderWrapper>.CreateInvalidResult(message.Identity, orderResult.Errors, null);
            }

            var order = this.Mapper.Map<Order>(orderResult.Data);

            order.SetIdentity(message.Identity);

            return ServiceMessage<OrderWrapper>.CreateValidResult(message.Identity, new OrderWrapper(order, orderResult.Data));
            #endregion
        }

        public async override Task<ServiceMessage> TryInvoiceOrderAsync(MarketplaceServiceMessage<(OrderStatusInvoice status, Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var invoice = this.Mapper.Map<MagaluInvoiceOrder>(message.Data.status);

            var invoiceResult = await this.Client.InvoiceOrder(invoice.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!invoiceResult.IsValid)
                result.WithErrors(invoiceResult.Errors);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryShipOrderAsync(MarketplaceServiceMessage<(OrderStatusShipment status, Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var shipment = this.Mapper.Map<MagaluShipOrder>(message.Data.status);

            var shipmentResult = await this.Client.ShipmentOrder(shipment.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!shipmentResult.IsValid)
                result.WithErrors(shipmentResult.Errors);

            return result;
            #endregion
        }

        public override async Task<ServiceMessage> NotifyOrderIntegrated(MarketplaceServiceMessage<Order> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            if (!message.Data.OrderStatus.Equals(OrderStatusEnum.Paid))
            {
                return result;
            }

            var orderStatus = this.Mapper.Map<MagaluOrderStatus>(message.Data);

            var orderStatusResult = await this.Client.ProcessingOrder(orderStatus.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!orderStatusResult.IsValid)
                result.WithErrors(orderStatusResult.Errors);

            return result;
            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Magalu;
        }

        public override Task<ServiceMessage> TryShipExceptionOrderAsync(MarketplaceServiceMessage<(OrderStatusShipException status, Order order)> message)
        {
            throw new NotImplementedException();
        }
    }
}
