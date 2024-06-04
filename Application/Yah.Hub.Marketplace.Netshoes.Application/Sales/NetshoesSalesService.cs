using AutoMapper;
using Yah.Hub.Application.Clients.ExternalClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.OrderService;
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
using Yah.Hub.Marketplace.Netshoes.Application.Client;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Order;
using Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus;
using Order = Yah.Hub.Marketplace.Netshoes.Application.Models.Order.Order;

namespace Yah.Hub.Marketplace.Netshoes.Application.Sales
{
    public class NetshoesSalesService : AbstractSalesService
    {
        #region [Properties]
        private readonly string Invoiced = "Invoiced";
        private readonly string Shipped = "Shipped";
        private readonly string Delivered = "Delivered";
        private readonly string Canceled = "Canceled";

        INetshoesClient Client { get; }
        #endregion

        public NetshoesSalesService(
            IConfiguration configuration,
            ILogger<AbstractSalesService> logger,
            IAccountConfigurationService configurationService,
            ICacheService cacheService,
            IOrderService orderService,
            IBrokerService brokerService,
            IERPClient eRPClient,
            INetshoesClient netshoesClient) 
            : base(configuration, logger, configurationService, cacheService, orderService, brokerService, eRPClient)
        {
            Client = netshoesClient;
        }

        public async override Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Domain.Order.Order> message)
        {
            #region [Code]
            return ServiceMessage.CreateValidResult(message.Identity);
            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            #region [Code]
            return MarketplaceAlias.Netshoes;
            #endregion
        }

        public async override Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]
            var result = new ServiceMessage<OrderWrapper>(message.Identity);

            var orderResult = await this.Client.GetOrder(message.Data.ToString().AsMarketplaceServiceMessage(message));

            if(!orderResult.IsValid && orderResult.Errors.Any())
            {
                result.WithErrors(orderResult.Errors);

                return result;
            }

            var order = Mapper.Map<Domain.Order.Order>(orderResult.Data);
            order.SetIdentity(message.Identity);

            result.WithData(new OrderWrapper(order, orderResult.Data));

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<List<Domain.Order.Order>>> GetOrdersToIntegrateFromMarketplaceAsync(MarketplaceServiceMessage<DateRange> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<Domain.Order.Order>>(message.Identity);
            HttpMarketplaceMessage<OrderResult> orderResult;
            var searchOrder = new SearchOrders(0,20,message.Data.From, message.Data.To);
            var netshoesOrders = new List<Order>();
            var orders = new List<Domain.Order.Order>();

            do
            {
                orderResult = await this.Client.GetOrders(searchOrder.AsMarketplaceServiceMessage(message));

                if (!orderResult.IsValid && orderResult.Errors.Any())
                {
                    result.WithErrors(orderResult.Errors);
                }

                if(orderResult.Data!= null && orderResult.Data.Items.Any())
                    netshoesOrders.AddRange(orderResult.Data.Items);

                searchOrder.Page++;


            }while (orderResult.Data != null && orderResult.Data.Items.Any());

            foreach(var netshoesOrder in netshoesOrders)
            {
                var order = Mapper.Map<Domain.Order.Order>(netshoesOrder);
                order.SetIdentity(message.Identity);
                orders.Add(order);
            }

            result.WithData(orders);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<ShipmentLabel>> GetShipmentLabelAsync(MarketplaceServiceMessage<string> message)
        {
            #region [Code]
            var result = new ServiceMessage<ShipmentLabel>(message.Identity);

            var request = new ShipmentLabelRequest() { DocumentType = "A4", ShippingCodes = new string[]  { message.Data.ToString() } };

            var requestResult = await this.Client.GetShipmentLabel(request.AsMarketplaceServiceMessage(message));

            if (!requestResult.IsValid && requestResult.Errors.Any())
            {
                result.WithErrors(requestResult.Errors);

                return result;
            }

            result.WithData(new ShipmentLabel() { URL = requestResult.Data.Tag.Url});

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryCancelOrderAsync(MarketplaceServiceMessage<(OrderStatusCancel status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var externalOrder = await this.Client.GetOrder(message.Data.order.IntegrationOrderId.AsMarketplaceServiceMessage(message));

            foreach (var shipping in externalOrder.Data.Shippings)
            {
                var wrapper = new OrderStatusWrapper<OrderCanceled>()
                {
                    Code = shipping.Code,
                    IntegrationOrderId = message.Data.order.IntegrationOrderId,
                    Status = Canceled,
                    Data = new OrderCanceled()
                    {
                        ReasonCancellationCode = message.Data.status.Reason,
                        Status = Canceled.ToLower()
                    }
                };

                var requestResult = await this.Client.ChangeOrderStatus(wrapper.AsMarketplaceServiceMessage(message));

                if (!requestResult.IsValid)
                {
                    if (requestResult.Errors.Any())
                    {
                        result.WithErrors(requestResult.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"erro ao tentar atualizar o status do pedido {message.Data.order.Id} para Faturado", "erro desconhecido", ErrorType.Technical));
                    }
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryDeliveryOrderAsync(MarketplaceServiceMessage<(OrderStatusDelivered status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var externalOrder = await this.Client.GetOrder(message.Data.order.IntegrationOrderId.AsMarketplaceServiceMessage(message));

            foreach (var shipping in externalOrder.Data.Shippings)
            {
                var wrapper = new OrderStatusWrapper<OrderDelivered>()
                {
                    Code = shipping.Code,
                    IntegrationOrderId = message.Data.order.IntegrationOrderId,
                    Status = Delivered,
                    Data = new OrderDelivered()
                    {
                        DeliveryDate = message.Data.status.DeliveryDate?.DateTime ?? DateTime.Now,
                        Status = Delivered
                    }
                };

                var requestResult = await this.Client.ChangeOrderStatus(wrapper.AsMarketplaceServiceMessage(message));

                if (!requestResult.IsValid)
                {
                    if (requestResult.Errors.Any())
                    {
                        result.WithErrors(requestResult.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"erro ao tentar atualizar o status do pedido {message.Data.order.Id} para Faturado", "erro desconhecido", ErrorType.Technical));
                    }
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryInvoiceOrderAsync(MarketplaceServiceMessage<(OrderStatusInvoice status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var externalOrder = await this.Client.GetOrder(message.Data.order.IntegrationOrderId.AsMarketplaceServiceMessage(message));

            foreach(var shipping in externalOrder.Data.Shippings)
            {
                var wrapper = new OrderStatusWrapper<OrderInvoiced>()
                {
                    Code = shipping.Code,
                    IntegrationOrderId = message.Data.order.IntegrationOrderId,
                    Status = Invoiced,
                    Data = new OrderInvoiced()
                    {
                        Number = message.Data.status.Number,
                        Line = message.Data.status.Series,
                        IssueDate = message.Data.status?.Date?.DateTime,
                        Key = message.Data.status.Key,
                        Status = Invoiced
                    }
                };

                var requestResult = await this.Client.ChangeOrderStatus(wrapper.AsMarketplaceServiceMessage(message));

                if (!requestResult.IsValid)
                {
                    if (requestResult.Errors.Any())
                    {
                        result.WithErrors(requestResult.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"erro ao tentar atualizar o status do pedido {message.Data.order.Id} para Faturado","erro desconhecido",ErrorType.Technical));
                    }
                }
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryShipOrderAsync(MarketplaceServiceMessage<(OrderStatusShipment status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var externalOrder = await this.Client.GetOrder(message.Data.order.IntegrationOrderId.AsMarketplaceServiceMessage(message));

            foreach (var shipping in externalOrder.Data.Shippings)
            {
                var wrapper = new OrderStatusWrapper<OrderShipped>()
                {
                    Code = shipping.Code,
                    IntegrationOrderId = message.Data.order.IntegrationOrderId,
                    Status = Shipped,
                    Data = new OrderShipped()
                    {
                        Carrier = message.Data.status.DeliveryMethod,
                        TrackingLink = message.Data.status.TrackingUrl,
                        TrackingNumber = message.Data.status.TrackingCode,
                        EstimatedDelivery = message.Data.status?.EstimatedTimeArrival?.DateTime,
                        DeliveredCarrierDate = message.Data.status?.Date?.DateTime,
                        Status = Shipped
                    }
                };

                var requestResult = await this.Client.ChangeOrderStatus(wrapper.AsMarketplaceServiceMessage(message));

                if (!requestResult.IsValid)
                {
                    if (requestResult.Errors.Any())
                    {
                        result.WithErrors(requestResult.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"erro ao tentar atualizar o status do pedido {message.Data.order.Id} para Faturado", "erro desconhecido", ErrorType.Technical));
                    }
                }
            }

            return result;
            #endregion
        }

        public override Task<ServiceMessage> TryShipExceptionOrderAsync(MarketplaceServiceMessage<(OrderStatusShipException status, Domain.Order.Order order)> message)
        {
            throw new NotImplementedException();
        }
    }
}
