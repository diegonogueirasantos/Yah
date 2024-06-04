using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.ShipmentLabel;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient
{
    public interface IBrokenClient
    {
        public Task<HttpMarketplaceMessage> RequestMessage<T>(MarketplaceServiceMessage<WrapperRequest<T>> message);
        public Task<HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceCategory>>>> GetCategories(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceCategory>>>> GetCategoryById(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> SaveAccountConfigurationSync(MarketplaceServiceMessage<Dictionary<string, string>> message);
        public Task<HttpMarketplaceMessage> ValidadeCredentials(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage<ServiceMessageResult<ShipmentLabel>>> GetShipmentLabel(MarketplaceServiceMessage<IOrderReference> message);
        public Task<HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceAttributes>>>> GetAttributes(MarketplaceServiceMessage<string?> message);
        public Task<HttpMarketplaceMessage> MonitorProductStatus(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage> ConsumeCommands(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage<ServiceMessageResult<List<IntegrationSummary>>>> GetIntegrationSummary(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage<ServiceMessageResult<EntityStateSearchResult>>> GetIntegrationByStatus(MarketplaceServiceMessage<EntityStateQuery> message);
    }
}
