using AutoMapper;
using Newtonsoft.Json;
using Yah.Hub.Application.Clients.ExternalClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.OrderService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.OrderRequest;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;
using Yah.Hub.Marketplace.Application.Broker;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.B2W.Application.Client.Interface;
using Yah.Hub.Marketplace.B2W.Application.Mappings;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Models.Order;
using System.Text;
using System.Xml;

namespace Yah.Hub.Marketplace.B2W.Application.Sales
{
    public class B2WSalesService : AbstractSalesService, ISalesService
    {
        private readonly IB2WClient Client;

        public B2WSalesService(
            IConfiguration configuration,
            ILogger<AbstractSalesService> logger,
            IAccountConfigurationService configurationService,
            ICacheService cacheService, IB2WClient client,
            IOrderService orderService,
            IBrokerService brokerService,
            IERPClient eRPClient) 
            : base(configuration, logger, configurationService, cacheService, orderService, brokerService, eRPClient)
        {
            this.Client = client;
        }

        public override async Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Order> message)
        {
            var result = new ServiceMessage(message.Identity);
            var dequeueResult = await this.Client.TryDequeueOrder(new MarketplaceServiceMessage<string>(message.Identity, message.AccountConfiguration, message.Data.IntegrationOrderId.ToString()));

            if (!dequeueResult.IsValid)
                result.WithErrors(dequeueResult.Errors);

            return result;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.B2W;
        }

        public override async Task<ServiceMessage<List<Order>>> GetOrdersToIntegrateFromMarketplaceAsync(MarketplaceServiceMessage<DateRange> message)
        {
            #region Code

            var result = new ServiceMessage<List<Order>>(message.Identity);
            var orderList = new List<B2WOrder>();
            var mappedOrders = new List<Order>();
            var shouldGetMore = true;

            // get orders from queue
            do
            {
                var getOrderResult = await this.Client.GetOrderFromQueue(message);

                if (getOrderResult.IsValid && getOrderResult.Data != null)
                {
                    orderList.Add(getOrderResult.Data);
                }
                else
                {
                    result.WithErrors(getOrderResult.Errors);
                    shouldGetMore = false;
                }
            } while (shouldGetMore);

            foreach (var order in orderList)
            {
                mappedOrders.Add(Mapper.Map<B2WOrder, Order>(order));
            }

            if(mappedOrders.Any())
                result.WithData(mappedOrders);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage<ShipmentLabel>> GetShipmentLabelAsync(MarketplaceServiceMessage<string> message)
        {
            var result = new ServiceMessage<ShipmentLabel>(message.Identity);

            // get shipment
            var shipmentResult = await this.Client.GetOrderShipment(message.Data.AsMarketplaceServiceMessage(message));

            if (!shipmentResult.IsValid)
            {
                result.WithError(new Error("Error while try to get shipment label from order", shipmentResult.Errors.First().Message, ErrorType.Technical));
                return result;
            }

            var plpId = string.Empty;

            // pedido já foi agrupado
            if(shipmentResult.Data?.plps?.FirstOrDefault()?.orders?.Any() ?? false)
            {
                plpId = shipmentResult.Data.plps.First().id.ToString();
            }
            // agrupar pedido
            else
            {
                var plp = new PlpGroup() { OrderIds = new List<string>() { message.Data } };
                var groupResult = await this.Client.GroupOrderShipments(plp.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));

                if(!groupResult.IsValid)
                {
                    result.WithError(new Error("Error while try to group shipment label from order", shipmentResult.Errors.First().Message, ErrorType.Technical));
                    return result;
                }

                plpId = groupResult.Data;
            }

            var shipmentLabel = await this.Client.GetShipmentLabel(plpId.AsMarketplaceServiceMessage(message.Identity, message.AccountConfiguration));
            result.WithData(new ShipmentLabel() { ByteArray = shipmentLabel.Data });
            return result;
        }

        public override async Task<ServiceMessage> TryCancelOrderAsync(MarketplaceServiceMessage<(OrderStatusCancel status, Order order)> message)
        {
            #region Code

            var result = new ServiceMessage(message.Identity);
            var b2wOrder = Mapper.Map<OrderStatusCancel, B2WCancelOrder>(message.Data.status);

            var cancelOrderResult = await this.Client.TryCancelOrder(new MarketplaceServiceMessage<B2WCancelOrder>(message.Identity, message.AccountConfiguration, b2wOrder));
            if (!cancelOrderResult.IsValid)
                result.WithErrors(cancelOrderResult.Errors);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage> TryDeliveryOrderAsync(MarketplaceServiceMessage<(OrderStatusDelivered status, Order order)> message)
        {
            #region Code

            var result = new ServiceMessage(message.Identity);
            var b2wOrder = Mapper.Map<OrderStatusDelivered, B2WDeliveryOrder>(message.Data.status);

            var cancelOrderResult = await this.Client.TryDeliveryOrder(new MarketplaceServiceMessage<B2WDeliveryOrder>(message.Identity, message.AccountConfiguration, b2wOrder));
            if (!cancelOrderResult.IsValid)
                result.WithErrors(cancelOrderResult.Errors);

            return result;

            #endregion
        }

        public override Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message)
        {
            throw new NotImplementedException();
        }

        public override async Task<ServiceMessage> TryInvoiceOrderAsync(MarketplaceServiceMessage<(OrderStatusInvoice status, Order order)> message)
        {
            #region Code

            var result = new ServiceMessage(message.Identity);
            var b2wOrder = Mapper.Map<OrderStatusInvoice, B2WInvoiceOrder>(message.Data.status);

            if (message.Data.order.Logistic.DeliveryLogisticType.Equals(DeliveryLogisticType.MarketplaceDelivery))
            {
                var invoiceOrderResult = await this.Client.TryInvoiceXMLOrder(new MarketplaceServiceMessage<(MultipartFormDataContent contentXML, string orderId)>
                                                                             (message.Identity, message.AccountConfiguration,(this.GenerateXML(message.Data.status.ContentXML), message.Data.status.OrderId)));
                if (!invoiceOrderResult.IsValid)
                    result.WithErrors(invoiceOrderResult.Errors);
            }
            else
            {
                var invoiceOrderResult = await this.Client.TryInvoiceOrder(new MarketplaceServiceMessage<B2WInvoiceOrder>(message.Identity, message.AccountConfiguration, b2wOrder));
                if (!invoiceOrderResult.IsValid)
                    result.WithErrors(invoiceOrderResult.Errors);
            }

            return result;

            #endregion
        }

        public override async Task<ServiceMessage> TryShipOrderAsync(MarketplaceServiceMessage<(OrderStatusShipment status, Order order)> message)
        {
            #region Code

            var result = new ServiceMessage(message.Identity);
            var b2wOrder = Mapper.Map<OrderStatusShipment, B2WShipOrder>(message.Data.status, opt =>
            {
                opt.Items[MappingContextKeys.Order] = message.Data.order;
            });

            var shipOrderResult = await this.Client.TryShipOrder(new MarketplaceServiceMessage<B2WShipOrder>(message.Identity, message.AccountConfiguration, b2wOrder));
            if (!shipOrderResult.IsValid)
                result.WithErrors(shipOrderResult.Errors);

            return result;

            #endregion
        }

        public override async Task<ServiceMessage> TryShipExceptionOrderAsync(MarketplaceServiceMessage<(OrderStatusShipException status, Order order)> message)
        {
            #region Code

            var result = new ServiceMessage(message.Identity);
            var b2wOrder = Mapper.Map<OrderStatusShipException, B2WShipExceptionOrder>(message.Data.status, opt =>
            {
                opt.Items[MappingContextKeys.Order] = message.Data.order;
            });

            var shipOrderResult = await this.Client.TryShipExceptionOrder(new MarketplaceServiceMessage<B2WShipExceptionOrder>(message.Identity, message.AccountConfiguration, b2wOrder));
            if (!shipOrderResult.IsValid)
                result.WithErrors(shipOrderResult.Errors);

            return result;

            #endregion
        }

        #region [Private]
        private MultipartFormDataContent GenerateXML(string invoiceXML)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(invoiceXML);
            var dhEmi = xmlDoc.GetElementsByTagName("dhEmi");
            var dhSaiEnt = xmlDoc.GetElementsByTagName("dhSaiEnt");

            var node = dhEmi.Equals(dhSaiEnt) ? dhEmi : dhSaiEnt;
            string issueDate = "";
            if (node != null && node.Count > 0) issueDate = node.Item(0)?.InnerText ?? "";

            var bytes = Encoding.Default.GetBytes(invoiceXML);
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new StringContent("order_invoiced"), "status");

            var fileContent = new ByteArrayContent(bytes, 0, bytes.Length);
            fileContent.Headers.TryAddWithoutValidation("Content-Type", "text/xml");
            form.Add(fileContent, "file", "fiscal_document.xml");

            form.Add(new StringContent(issueDate), "issue_date");
            form.Add(new StringContent("1"), "volume_qty");

            return form;
        }
        #endregion
    }
}
