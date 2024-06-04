using Amazon.DynamoDBv2.DocumentModel;
using Nest;
using AutoMapper;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.OrderRequest;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.MercadoLivre.Application.Client;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales;
using System.Collections.Generic;
using DateRange = Yah.Hub.Common.OrderRequest.DateRange;
using Yah.Hub.Common.Enums;
using System.Text;
using Yah.Hub.Domain.ShipmentLabel;
using System.Linq;
using Yah.Hub.Application.Services.OrderService;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Application.Clients.ExternalClient;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Sales
{
    public class MercadoLivreSalesService : AbstractSalesService, ISalesService
    {

        private readonly int OrderPerPage = 50;
        private const int _DELIVERY_DEFAULT_ID = 11;

        private IMercadoLivreClient Client { get; }

        public MercadoLivreSalesService(IConfiguration configuration, ILogger<AbstractSalesService> logger, IAccountConfigurationService configurationService, ICacheService cacheService, IOrderService orderService, IMercadoLivreClient client, IBrokerService brokerService, IERPClient eRPClient) : base(configuration, logger, configurationService, cacheService, orderService, brokerService, eRPClient)
        {
            this.Client = client;
        }

        public override async Task<ServiceMessage<List<Order>>> GetOrdersToIntegrateFromMarketplaceAsync(MarketplaceServiceMessage<DateRange> message)
        {
            #region [Code]

            var ordersId = new List<long>();
            var orders = new List<Order>();
            var meliOrders = new List<MeliOrder>();

            var statuses = new[] { "PAID", "CANCELLED", "SHIPPED" };

            var query = new OrderQueryRequest(message.AccountConfiguration.AppId, 0, OrderPerPage, message.Data.From, message.Data.To);

            HttpMarketplaceMessage<OrderClientResult> orderResult;

            var result = new ServiceMessage<List<Order>>(message.Identity);

            foreach (var status in statuses)
            {
                var offset = 0;
                var filter = GetOrderStatusFilter(status);

                query.Status.Add(filter.Key, filter.Value);

                do
                {
                    query.Offset = offset;

                    orderResult = await this.Client.GetOrdersForIntegration(query.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                    query.Status.Remove(filter.Key);

                    if (!orderResult.IsValid)
                    {
                        if (ordersId.Count() > 0)
                        {
                            break;
                        }
                        else
                        {
                            result.WithErrors(orderResult.Errors ?? new List<Error>());

                            return result;
                        }
                    }

                    if (orderResult.Data.Results.Count() > 0)
                    {
                        ordersId.AddRange(orderResult.Data.Results.Select(x => x.Id).ToList());
                    }

                    offset += orderResult.Data.Results.Count();

                    if (orderResult.Data.Results.Count() < OrderPerPage)
                    {
                        break;
                    }

                } while (orderResult.IsValid && offset > 0);
            }

            foreach (var meliOrder in ordersId)
            {
                var meliOrderResult = await this.GetMeliOrder(meliOrder.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if (meliOrderResult.IsValid)
                    meliOrders.Add(meliOrderResult.Data);
            }


            Parallel.ForEach(meliOrders, order =>
            {
                var orderMapped = Mapper.Map<Order>(order);
                orders.Add(orderMapped);
            });

            result.WithData(orders);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage> TryCancelOrderAsync(MarketplaceServiceMessage<(OrderStatusCancel status, Order order)> message)
        {
            #region [Code]

            return ServiceMessage.CreateInvalidResult(message.Identity, new Error("Operção não é suportada pelo marketplace.", "Operção não é suportada pelo marketplace.", ErrorType.Business));

            #endregion
        }

        public override async Task<ServiceMessage> TryDeliveryOrderAsync(MarketplaceServiceMessage<(OrderStatusDelivered status, Order order)> message)
        {
            #region [Code]

            var result = ServiceMessage.CreateValidResult(message.Identity);

            var orderResult = await this.TryGetOrderFromMarketplaceAsync(message.Data.status.AsMarketplaceServiceMessage<IOrderReference>(message.Identity, message.AccountConfiguration));

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }

            if (!this.CanOrderAction(orderResult.Data.Order.OrderStatus, OrderStatusEnum.Delivered))
            {
                result.WithError(new Error($"Não é possível realizar a transição de status de {orderResult.Data.Order.OrderStatus} para {OrderStatusEnum.Delivered}", "Transição de status inválida", ErrorType.Business));
            }

            var updateResult = await this.Client.DeliveryOrder(message.Data.status.IntegrationOrderId.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!updateResult.IsValid)
            {
                result.WithErrors(updateResult.Errors);
                return result;
            }

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]

            var result = new ServiceMessage<OrderWrapper>(message.Identity);

            var orderResult = await this.Client.GetOrder(message.Data.IntegrationOrderId.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }

            var order = Mapper.Map<Order>(orderResult.Data);

            return ServiceMessage<OrderWrapper>.CreateValidResult(message.Identity, new OrderWrapper(order, orderResult.Data));

            #endregion
        }

        public override async Task<ServiceMessage> TryInvoiceOrderAsync(MarketplaceServiceMessage<(OrderStatusInvoice status, Order order)> message)
        {
            #region [Code]

            var result = ServiceMessage.CreateValidResult(message.Identity);

            MarketplaceServiceMessage updateResult = null;

            var orderResult = await this.TryGetOrderFromMarketplaceAsync(message.Data.status.AsMarketplaceServiceMessage<IOrderReference>(message.Identity, message.AccountConfiguration));

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }

            if (!this.CanOrderAction(orderResult.Data.Order.OrderStatus, OrderStatusEnum.Invoiced))
            {
                result.WithError(new Error($"Não é possível realizar a transição de status de {orderResult.Data.Order.OrderStatus} para {OrderStatusEnum.Delivered}", "Transição de status inválida", ErrorType.Business));
            }

            var meliOrder = orderResult.Data.MarketplaceOrder as MeliOrder;

            bool isME2 = meliOrder.Shipping?.Logistic?.Mode?.Equals("me2", StringComparison.InvariantCultureIgnoreCase) ?? false;

            if (!isME2)
            {
                if (!string.IsNullOrWhiteSpace(message.Data.status.ContentXML) && meliOrder.PackId.HasValue && !(meliOrder.FiscalDocuments?.FiscalDocuments.Any() ?? false))
                {
                    var xml = Encoding.Default.GetBytes(message.Data.status.ContentXML);

                    string fileName = "nf_upload_" +
                                      DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                                      "_" +
                                      message.Identity.GetTenantId() +
                                      "_" +
                                      message.Identity.GetAccountId() +
                                      ".xml";

                    updateResult = await this.Client.InvoiceOrder(new MarketplaceServiceMessage<(string packId, byte[] xml, string fileName)>(message.Identity, message.AccountConfiguration, (meliOrder.PackId.ToString(), xml, fileName)));

                    if (!updateResult.IsValid)
                    {
                        result.WithErrors(updateResult.Errors);
                        return result;
                    }
                }

                return result;
            }

            if (!string.IsNullOrWhiteSpace(message.Data.status.ContentXML))
            {
                updateResult = await this.Client.InvoiceOrder(new MarketplaceServiceMessage<(string shippingId, string invoiceXML)>(message.Identity, message.AccountConfiguration, (meliOrder.Shipping.Id.ToString(), message.Data.status.ContentXML)));
            }
            else
            {
                var invoiceMapped = Mapper.Map<MeliInvoice>(message.Data.status);

                updateResult = await this.Client.InvoiceOrder(new MarketplaceServiceMessage<(string shippingId, MeliInvoice invoice)>(message.Identity, message.AccountConfiguration, (meliOrder.Shipping.Id.ToString(), invoiceMapped)));
            }

            if (!updateResult.IsValid)
                result.WithErrors(updateResult.Errors);


            return result;

            #endregion
        }

        public override async Task<ServiceMessage> TryShipOrderAsync(MarketplaceServiceMessage<(OrderStatusShipment status, Order order)> message)
        {
            #region [Code]

            var result = ServiceMessage.CreateValidResult(message.Identity);

            var orderResult = await this.TryGetOrderFromMarketplaceAsync(message.Data.status.AsMarketplaceServiceMessage<IOrderReference>(message.Identity, message.AccountConfiguration));

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }

            if (!this.CanOrderAction(orderResult.Data.Order.OrderStatus, OrderStatusEnum.Shipped))
            {
                result.WithError(new Error($"Não é possível realizar a transição de status de {orderResult.Data.Order.OrderStatus} para {OrderStatusEnum.Delivered}", "Transição de status inválida", ErrorType.Business));
            }

            var meliOrder = orderResult.Data.MarketplaceOrder as MeliOrder;

            var serviceId = this.SetDeliveryMethod(message.Data.status.DeliveryMethod);

            var updateResult = await this.Client.ShippedOrder(new MarketplaceServiceMessage<(string shippingId, string trackingNumber, int serviceId, long? buyer)>(message.Identity, message.AccountConfiguration, (meliOrder.Shipping.Id.ToString(), message.Data.status.TrackingCode, serviceId, meliOrder.Buyer.Id)));

            if (!updateResult.IsValid)
                result.WithErrors(updateResult.Errors);

            return result;

            #endregion
        }

        public async Task<ServiceMessage<MeliOrder>> GetMeliOrder(MarketplaceServiceMessage<long> message)
        {
            #region [Code]

            var result = new ServiceMessage<MeliOrder>(message.Identity);

            var orderResult = await this.GetMeliOrderBaseInfo(message);

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }
            result.WithData(orderResult.Data);

            return result;

            #endregion
        }

        public async Task<ServiceMessage<MeliOrder>> GetMeliOrderBaseInfo(MarketplaceServiceMessage<long> message)
        {
            #region [Code]

            var result = new ServiceMessage<MeliOrder>(message.Identity);

            var orderResult = await this.Client.GetOrder(message.Data.ToString().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }

            var order = orderResult.Data;

            #region Get Shipping

            if (order.Shipping.Id != null && order.Shipping.Id != default)
            {
                var shippingResult = await this.Client.GetMeliShipping(order.Shipping.Id.ToString().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if (!shippingResult.IsValid)
                {
                    result.WithErrors(shippingResult.Errors);
                    return result;
                }

                order.Shipping = shippingResult.Data;
            }

            #endregion

            #region Get BillingInfo

            var billingResult = await this.Client.GetBillingInfo(message.Data.ToString().AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

            if (!billingResult.IsValid)
            {
                result.WithErrors(billingResult.Errors);
                return result;
            }

            order.Buyer.BillingInfo = billingResult.Data?.BillingInfo;

            #endregion

            result.WithData(order);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<ShipmentLabel>> GetShipmentLabelAsync(MarketplaceServiceMessage<string> message)
        {
            var result = new ServiceMessage<ShipmentLabel>(message.Identity);

            var orderResult = await this.GetMeliOrderBaseInfo(new MarketplaceServiceMessage<long>(message.Identity, message.AccountConfiguration, Convert.ToInt64(message.Data)));

            if (!orderResult.IsValid)
            {
                result.WithErrors(orderResult.Errors);
                return result;
            }

            //orderResult.Data.Shipping.Id
            var shipmentLabelResult = await this.Client.GetShipmentLabel(new MarketplaceServiceMessage<string>(message.Identity, message.AccountConfiguration, orderResult.Data.Shipping.Id.ToString()));

            if (!shipmentLabelResult.IsValid)
            {
                result.WithErrors(shipmentLabelResult.Errors);
                return result;
            }

            result.WithData(new ShipmentLabel()
            {
                ByteArray = shipmentLabelResult.Data,
                URL = "" // TODO: REVIEW THIS PROPERTY
            });

            return result;
        }

        #region Private

        private KeyValuePair<string, string> GetOrderStatusFilter(string status)
        {
            #region [Code]
            switch (status)
            {
                case "PAID":
                case "CANCELLED":
                    return new KeyValuePair<string, string>("order.status", status.ToLower());
                    break;
                case "SHIPPED":
                case "DELIVERED":
                    return new KeyValuePair<string, string>("shipping.status", status.ToLower());
                    break;
            }

            return default;
            #endregion
        }

        private bool CanOrderAction(OrderStatusEnum orderStatusFrom, OrderStatusEnum orderStatusTo)
        {
            #region [Code]
            return
                (orderStatusTo == OrderStatusEnum.Invoiced && (orderStatusFrom == OrderStatusEnum.Paid))
                ||
                (orderStatusTo == OrderStatusEnum.Shipped && (orderStatusFrom == OrderStatusEnum.Invoiced || orderStatusFrom == OrderStatusEnum.Paid))
                ||
                (orderStatusTo == OrderStatusEnum.Delivered && (orderStatusFrom == OrderStatusEnum.Shipped || orderStatusFrom == OrderStatusEnum.Paid))
                ||
                (orderStatusTo == OrderStatusEnum.Canceled);
            #endregion
        }

        private static readonly Dictionary<int, string> _DELIVERY_SERVICES = new Dictionary<int, string>
        {
            #region [Code]

            { 21,"PAC" },
            { 23,"eSedex" },
            { 22,"Sedex" },
            { 261,"Cougar Normal" },
            { 262,"Cougar Expresso" },
            { 821,"DHL" },
            { 691,"Total Medio Rodo" },
            { 1041,"Correios Sedex Agência" },
            { 293,"Fulfillment Express" },
            { 841,"Loggi a domicilio" },
            { 741,"CBT DHL" },
            { 291,"Fulfillment Normal" },
            { 138276,"Testing PUT" },
            { 1061,"Coleta Correios Sedex Agência" },
            { 292,"Fulfillment Express" },
            { 263,"Jadlog Normal" },
            { 264,"Jadlog Expresso" },
            { 11,"Outros" },
            { 101,"Coleta Normal" },
            { 102,"Coleta Express" },
            { 103,"Coleta Express" },
            { 282,"CBT DDU" },
            { 761,"SuperExpress" },
            { 161,"CBT" },
            { 991,"Mercadoenvios Next Day" },
            { 104,"DGT Normal" },
            { 105,"Total Normal" },
            { 106,"Transfolha Normal" },
            { 107,"Directlog Normal" },
            { 108,"Directlog Express" },
            { 109,"Transfolha Express" },
            { 110,"Total Express" },
            { 751,"CBT MA" },
            { 1051,"Coleta Correios PAC Agência" },
            { 1171,"FedEx Normal" },
            { 301,"DGT Expresso" },
            { 871,"Loggi Express a domicilio" },
            { 1031,"Correios PAC Agência" },
            { 891,"Estandar" },
            { 136661,"CBT EL" },
            { 135551,"Loggi next day a domicilio" },
            { 135811,"Plimor Normal" },
            { 135821,"Plimor Expresso" },
            { 145961,"Azul Cargo Normal" },
            { 146061,"LATAM Cargo Normal" }

            #endregion
        };

        private int SetDeliveryMethod(string deliveryMethod)
        {
            #region [Code]

            if (_DELIVERY_SERVICES.Any(x => deliveryMethod.ToLower().Equals(x.Value.ToLower())))
                return _DELIVERY_SERVICES.First(x => deliveryMethod.ToLower().Equals(x.Value.ToLower())).Key;

            return _DELIVERY_DEFAULT_ID;

            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }

        public override async Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Order> message)
        {
            return ServiceMessage.CreateValidResult(message.Identity);
        }

        public override Task<ServiceMessage> TryShipExceptionOrderAsync(MarketplaceServiceMessage<(OrderStatusShipException status, Order order)> message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
