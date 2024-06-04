using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Marketplace.Netshoes.Application.Models;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Order;
using Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus;

namespace Yah.Hub.Marketplace.Netshoes.Application.Client
{
    public interface INetshoesClient
    {
        public Task<HttpMarketplaceMessage<Product>> GetProduct(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> CreateProduct(MarketplaceServiceMessage<Product> message);
        public Task<HttpMarketplaceMessage> UpdateProduct(MarketplaceServiceMessage<Product> message);
        public Task<HttpMarketplaceMessage<Price>> GetPrice(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> CreatePrice(MarketplaceServiceMessage<(Price Price, string Sku)> message);
        public Task<HttpMarketplaceMessage> UpdatePrice(MarketplaceServiceMessage<(Price Price, string Sku)> message);
        public Task<HttpMarketplaceMessage<Stock>> GetStock(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> CreateStock(MarketplaceServiceMessage<(Stock Stock, string Sku)> message);
        public Task<HttpMarketplaceMessage> UpdateStock(MarketplaceServiceMessage<(Stock Stock, string Sku)> message);
        public Task<HttpMarketplaceMessage<Order>> GetOrder(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> ChangeOrderStatus<T>(MarketplaceServiceMessage<OrderStatusWrapper<T>> message);
        public Task<HttpMarketplaceMessage<OrderResult>> GetOrders(MarketplaceServiceMessage<SearchOrders> message);
        public Task<HttpMarketplaceMessage<ShipmentLabelResult>> GetShipmentLabel(MarketplaceServiceMessage<ShipmentLabelRequest> message);
        public Task<HttpMarketplaceMessage<CategoriesTree>> GetCategories(MarketplaceServiceMessage<string?> message);
        public Task<HttpMarketplaceMessage<AttributeGroup>> GetAttributes(MarketplaceServiceMessage<(string categoryId, string attributeID)> message);
        public Task<HttpMarketplaceMessage<GenericAttributes>> GetTemplateAttributes(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> TryAuthenticate(MarketplaceServiceMessage message);

    }
}
