using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Monitor;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerMonitorService
{
    public interface IBrokerMonitorService
    {
        public Task<ServiceMessage> MonitorProductStatus(MarketplaceServiceMessage message);
        public Task<ServiceMessage> ConsumeCommands(MarketplaceServiceMessage message);
        public Task<ServiceMessage<List<IntegrationSummary>>> GetIntegrationSummary(MarketplaceServiceMessage message);
        public Task<ServiceMessage<EntityStateSearchResult>> GetIntegrationByStatus(MarketplaceServiceMessage<EntityStateQuery> message);
    }
}
