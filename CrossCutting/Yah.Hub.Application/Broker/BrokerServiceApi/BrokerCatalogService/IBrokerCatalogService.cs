using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCatalogService
{
    public interface IBrokerCatalogService
    {
        public Task<ServiceMessage> RequestAsync<T>(MarketplaceServiceMessage<(T RequestData, Operation OperationId)> message);
    }
}
