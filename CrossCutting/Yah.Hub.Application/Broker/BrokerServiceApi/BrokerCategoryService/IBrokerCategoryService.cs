using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCategoryService
{
    public interface IBrokerCategoryService
    {
        public Task<ServiceMessage<List<MarketplaceCategory>>> GetCategory(MarketplaceServiceMessage<string> message);
        public Task<ServiceMessage<List<MarketplaceCategory>>> GetCategories(MarketplaceServiceMessage message);
        public Task<ServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetAttribute(MarketplaceServiceMessage<string> message);
    }
}
