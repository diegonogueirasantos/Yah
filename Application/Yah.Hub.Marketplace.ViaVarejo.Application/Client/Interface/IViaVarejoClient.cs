using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Category;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface
{
    public interface IViaVarejoClient
    {
        public Task<HttpMarketplaceMessage> UpsertProduct(MarketplaceServiceMessage<ProductWrapper> message);
        public Task<HttpMarketplaceMessage<Product>> GetSkuStatus(MarketplaceServiceMessage<(string productId, string skuId)> message);
        public Task<HttpMarketplaceMessage> DeleteProduct(MarketplaceServiceMessage<(string productId, string skuId)> message);
        public Task<HttpMarketplaceMessage> UpdatePrice(MarketplaceServiceMessage<PriceUpdate> message);
        public Task<HttpMarketplaceMessage> UpdateInventory(MarketplaceServiceMessage<InventoryUpdate> message);
        public Task<HttpMarketplaceMessage> SetSkuOption(MarketplaceServiceMessage<SkuOption> message);
        public Task<HttpMarketplaceMessage> UpdateProductStatus(MarketplaceServiceMessage<UpdateStatus> message);
        public Task<HttpMarketplaceMessage<CategoryTree>> GetCategories(MarketplaceServiceMessage<SearchCategory> message);
        public Task<HttpMarketplaceMessage<OrderResult>> GetOrders(MarketplaceServiceMessage<SearchOrders> message);
        public Task<HttpMarketplaceMessage<Order>> GetOrder(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> SetOrderStatus<T>(MarketplaceServiceMessage<UpdateOrderStatus<T>> message);
        public Task<HttpMarketplaceMessage<ShipmentLabel>> GetShipmentLabel(MarketplaceServiceMessage<ShipmentLabelRequest> message);
        public Task<HttpMarketplaceMessage> ValidateCredentials(MarketplaceServiceMessage message);
    }
}
