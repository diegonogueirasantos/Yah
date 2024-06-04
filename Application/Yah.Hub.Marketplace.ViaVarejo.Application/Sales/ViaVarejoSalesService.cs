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
using Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Mappings;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order;
using Invoice = Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order.Invoice;
using Order = Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order.Order;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Sales
{
    public class ViaVarejoSalesService : AbstractSalesService, ISalesService
    {
        IViaVarejoClient Client { get; }
        ICacheService CacheService { get; }

        private readonly int OrderLimit = 10;
        private readonly string Invoice = "invoice";
        private readonly string Sent = "sent";
        private readonly string Delivered = "delivered";

        public ViaVarejoSalesService(
            IConfiguration configuration,
            ILogger<AbstractSalesService> logger,
            IAccountConfigurationService configurationService,
            ICacheService cacheService,
            IOrderService orderService,
            IViaVarejoClient viaVarejoClient,
            IBrokerService brokerService,
            IERPClient eRPClient) 
            : base(configuration, logger, configurationService, cacheService, orderService, brokerService, eRPClient)
        {
            Client = viaVarejoClient;
            CacheService = cacheService;
        }

        public async override Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Domain.Order.Order> message)
        {
            return ServiceMessage.CreateValidResult(message.Identity);
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.ViaVarejo;
        }

        public async override Task<ServiceMessage<List<Domain.Order.Order>>> GetOrdersToIntegrateFromMarketplaceAsync(MarketplaceServiceMessage<DateRange> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<Domain.Order.Order>>(message.Identity);
            var orders = new List<Domain.Order.Order>();
            HttpMarketplaceMessage<OrderResult> requestResult;
            var statuses = new string[] { "new", "approved", "sent", "delivered", "canceled" };

            foreach (var status in statuses)
            {
                var searchOrder = new SearchOrders(OrderLimit, 0, message.Data.From, message.Data.To, status);

                do
                {
                    requestResult = await this.Client.GetOrders(searchOrder.AsMarketplaceServiceMessage(message));

                    if(requestResult.Data == null || !requestResult.IsValid)
                    {
                        if (requestResult.Errors.Any())
                        {
                            result.WithErrors(requestResult.Errors);

                            if (orders.Any())
                                result.WithData(orders);

                            return result;
                        }
                        else
                        {
                            result.WithError(new Error($"Erro ao tentar obter os pedidos do status {status} no marketplace Via Varejo.", $"Erro desconhecido, statuscode: {requestResult.StatusCode}", ErrorType.Technical));

                            if (orders.Any())
                                result.WithData(orders);

                            return result;
                        }
                    }

                    foreach(var order in requestResult.Data.Orders)
                    {
                        orders.Add(Mapper.Map<Domain.Order.Order>(order));
                    }

                    searchOrder.Offset += OrderLimit;

                } while (requestResult.IsValid && requestResult.Data.Orders.Length == OrderLimit);
            }

            result.WithData(orders);

            return result;
            #endregion
        }

        public async override Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]
            var result = new ServiceMessage<OrderWrapper>(message.Identity);

            var requestResult = await this.Client.GetOrder(message.Data.IntegrationOrderId.ToString().AsMarketplaceServiceMessage(message));

            if (requestResult.Data == null || !requestResult.IsValid)
                result.WithErrors(requestResult.Errors);
            else
                result.WithData(new OrderWrapper(Mapper.Map<Domain.Order.Order>(requestResult.Data), requestResult.Data));
            
            return result;
            #endregion
        }

        public async override Task<ServiceMessage<Domain.ShipmentLabel.ShipmentLabel>> GetShipmentLabelAsync(MarketplaceServiceMessage<string> message)
        {
            #region [Code]
            var result = new ServiceMessage<Domain.ShipmentLabel.ShipmentLabel>(message.Identity);

            var resultRequest = await this.Client.GetShipmentLabel(new ShipmentLabelRequest(message.Data).AsMarketplaceServiceMessage(message));

            result.WithData(new Domain.ShipmentLabel.ShipmentLabel()
            {
                URL = resultRequest.Data.Labels.FirstOrDefault().Pdf,
            });

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryCancelOrderAsync(MarketplaceServiceMessage<(OrderStatusCancel status, Domain.Order.Order order)> message)
        {
            #region [Code]
            return ServiceMessage.CreateInvalidResult(message.Identity, new Error("Não é possível cancelar um pedido no Marketplace Via Varejo através do Integrador." +
                                                                                  "É necessário acessar o painel de lojista no Marketplace para realizar o cancelamento.","", ErrorType.Business));
            #endregion
        }

        public async override Task<ServiceMessage> TryDeliveryOrderAsync(MarketplaceServiceMessage<(OrderStatusDelivered status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);
            var order = message.Data.order;

            var shipmentInfo = await this.CacheService.Get<OrderShipment>(new ServiceMessage<string>(message.Identity, $"Ship-{order.IntegrationOrderId}"));

            shipmentInfo.Data.OccurredAt = DateTime.Now.ToString();

            var requestResult = await this.Client.SetOrderStatus(new UpdateOrderStatus<OrderShipment>(shipmentInfo.Data, Delivered, order.IntegrationOrderId).AsMarketplaceServiceMessage(message));

            if (!requestResult.IsValid)
            {
                if (requestResult.Errors.Any())
                {
                    result.WithErrors(requestResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar atualizar o pedido {order.IntegrationOrderId} para Entregue no marketplace Via Varejo.", $"Erro desconhecido, statuscode: {requestResult.StatusCode}", ErrorType.Technical));
                }
            }
            else
            {
                await this.CacheService.Remove(new ServiceMessage<string>(message.Identity, $"Ship-{order.IntegrationOrderId}"));
            }

            return result;
            #endregion
        }

        public async override Task<ServiceMessage> TryInvoiceOrderAsync(MarketplaceServiceMessage<(OrderStatusInvoice status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);
            var order = message.Data.order;

            var invoiceInfo = Mapper.Map<InvoiceOrder>(message.Data.status, opt =>
            {
                opt.Items[MappingContextKeys.Order] = order;
            });

            var requestResult = await this.Client.SetOrderStatus(new UpdateOrderStatus<InvoiceOrder>(invoiceInfo, Invoice, order.IntegrationOrderId).AsMarketplaceServiceMessage(message));
            
            if (!requestResult.IsValid)
            {
                if (requestResult.Errors.Any())
                {
                    result.WithErrors(requestResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar atualizar o pedido {order.IntegrationOrderId} para Faturado no marketplace Via Varejo.", $"Erro desconhecido, statuscode: {requestResult.StatusCode}", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public override Task<ServiceMessage> TryShipExceptionOrderAsync(MarketplaceServiceMessage<(OrderStatusShipException status, Domain.Order.Order order)> message)
        {
            throw new NotImplementedException();
        }

        public async override Task<ServiceMessage> TryShipOrderAsync(MarketplaceServiceMessage<(OrderStatusShipment status, Domain.Order.Order order)> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);
            var order = message.Data.order;

            var shipmentInfo = Mapper.Map<OrderShipment>(message.Data.status, opt =>
            {
                opt.Items[MappingContextKeys.Order] = order;
            });

            var requestResult = await this.Client.SetOrderStatus(new UpdateOrderStatus<OrderShipment>(shipmentInfo, Sent, order.IntegrationOrderId).AsMarketplaceServiceMessage(message));

            if (!requestResult.IsValid)
            {
                if (requestResult.Errors.Any())
                {
                    result.WithErrors(requestResult.Errors);
                }
                else
                {
                    result.WithError(new Error($"Erro ao tentar atualizar o pedido {order.IntegrationOrderId} para Enviado no marketplace Via Varejo.", $"Erro desconhecido, statuscode: {requestResult.StatusCode}", ErrorType.Technical));
                }
            }
            else
            {
                //cache for deliveryOrder after
                await this.CacheService.Set(new ServiceMessage<(string key, OrderShipment value, TimeSpan? expires, StackExchange.Redis.When? when)>
                                                                             (message.Identity, ($"Ship-{order.IntegrationOrderId}", shipmentInfo, TimeSpan.FromDays(60), StackExchange.Redis.When.NotExists)));
            }

            return result;
            #endregion
        }
    }
}
