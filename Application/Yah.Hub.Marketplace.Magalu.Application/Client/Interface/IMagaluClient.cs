using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Formatting;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Magalu.Application.Models;
using Yah.Hub.Marketplace.Magalu.Application.Models.Order;
using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Marketplace.Magalu.Application.Client.Interface
{
    public interface IMagaluClient
    {
        public Task<HttpMarketplaceMessage> ValidateAuthorization(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage> CreateProduct(MarketplaceServiceMessage<Product> message);
        public Task<HttpMarketplaceMessage> CreateSku(MarketplaceServiceMessage<Sku> message);
        public Task<HttpMarketplaceMessage> UpdateProduct(MarketplaceServiceMessage<Product> message);
        public Task<HttpMarketplaceMessage> UpdateSku(MarketplaceServiceMessage<Sku> message);
        public Task<HttpMarketplaceMessage> UpdatePrice(MarketplaceServiceMessage<Price[]> message);
        public Task<HttpMarketplaceMessage> UpdateStock(MarketplaceServiceMessage<Stock[]> message);
        public Task<HttpMarketplaceMessage<Product>> GetProduct(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage<Sku>> GetSku(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> CreateCategory(MarketplaceServiceMessage<Category[]> message);
        public Task<HttpMarketplaceMessage<MagaluOrderQueue>> GetOrdersFromQueue(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage<MagaluOrder>> GetOrder(MarketplaceServiceMessage<IOrderReference> message);
        public Task<HttpMarketplaceMessage> DequeueOrders(MarketplaceServiceMessage<OrderQueueItemId[]> message);
        public Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<MagaluInvoiceOrder> message);
        public Task<HttpMarketplaceMessage> ShipmentOrder(MarketplaceServiceMessage<MagaluShipOrder> message);
        public Task<HttpMarketplaceMessage> DeliveryOrder(MarketplaceServiceMessage<MagaluDeliveryOrder> message);
        public Task<HttpMarketplaceMessage> ProcessingOrder(MarketplaceServiceMessage<MagaluOrderStatus> message);
        public Task<HttpMarketplaceMessage<MagaluShipmentLabel[]>> GetShipmentLabel(MarketplaceServiceMessage<MagaluShipmentLabelRequest> message);
        Task<HttpMarketplaceMessage<List<MagaluApiLimit>>> GetApiLimit(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage<ResponseToken>> AuthenticationTokenAsync(MarketplaceServiceMessage<Dictionary<string, string>> message);
        public Task<HttpMarketplaceMessage> FinalizedAuthenticationAsync(MarketplaceServiceMessage message);
    }
}
