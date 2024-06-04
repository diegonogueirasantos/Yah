using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Formatting;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Models.Order;

namespace Yah.Hub.Marketplace.B2W.Application.Client.Interface
{
    public interface IB2WClient
    {
        #region Product

        public Task<HttpMarketplaceMessage> CreateProduct(MarketplaceServiceMessage<Product> message);

        public Task<HttpMarketplaceMessage> UpdateProduct(MarketplaceServiceMessage<Product> message);

        public Task<HttpMarketplaceMessage<Product>> GetProduct(MarketplaceServiceMessage<string> message);

        public Task<HttpMarketplaceMessage> UpdateVariation(MarketplaceServiceMessage<Variation> message);

        public Task<HttpMarketplaceMessage<B2WErrors>> GetProductErrors(MarketplaceServiceMessage<string> message);

        #endregion

        #region Order

        public Task<HttpMarketplaceMessage<B2WOrder>> GetOrderFromQueue(MarketplaceServiceMessage marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage<B2WOrder>> GetOrder(MarketplaceServiceMessage marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryInvoiceOrder(MarketplaceServiceMessage<B2WInvoiceOrder> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryInvoiceXMLOrder(MarketplaceServiceMessage<(MultipartFormDataContent contentXML, string orderId)> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryShipOrder(MarketplaceServiceMessage<B2WShipOrder> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryDeliveryOrder(MarketplaceServiceMessage<B2WDeliveryOrder> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryCancelOrder(MarketplaceServiceMessage<B2WCancelOrder> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryDequeueOrder(MarketplaceServiceMessage<string> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage<Shipment>> GetOrderShipment(MarketplaceServiceMessage<string> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> TryShipExceptionOrder(MarketplaceServiceMessage<B2WShipExceptionOrder> marketplaceServiceMessage);
        /// <summary>
        /// TODO: ESTE MÉTODO PELA DOCUMENTAÇÃO RETORNA APENAS UMA MENSAGEM COM O ID DA PLP DENTRO DELA EM UM FORMATO STRING, VERIFICAR SE CONDIZ COM A REALIDADE
        /// </summary>
        /// <param name="marketplaceServiceMessage"></param>
        /// <returns></returns>
        public Task<HttpMarketplaceMessage<string>> GroupOrderShipments(MarketplaceServiceMessage<PlpGroup> marketplaceServiceMessage);


        public Task<HttpMarketplaceMessage<byte[]>> GetShipmentLabel(MarketplaceServiceMessage<string> marketplaceServiceMessage);
        #endregion

        #region Configuration

        public Task<HttpMarketplaceMessage<AuthResponse>> RenewRehubToken(MarketplaceServiceMessage<Auth> marketplaceServiceMessage);
        public Task<HttpMarketplaceMessage<ProductLinkResult>> LinkProduct(MarketplaceServiceMessage<ProductLink> marketplaceServiceMessage);

        #endregion
    }
}
