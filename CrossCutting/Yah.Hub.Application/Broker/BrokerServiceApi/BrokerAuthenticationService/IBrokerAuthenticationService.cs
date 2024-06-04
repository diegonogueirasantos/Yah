using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerAuthenticationService
{
    public interface IBrokerAuthenticationService
    {
        public Task<ServiceMessage> SaveAccountConfiguration(MarketplaceServiceMessage<Dictionary<string, string>> message);
        public Task<ServiceMessage> ValidadeCredentials(MarketplaceServiceMessage message);
    }
}
