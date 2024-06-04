using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Broker.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Messages
{
    public class PeekCommandMessage<T> : MarketplaceIdentity, IBrokerMessage
    {
        public PeekCommandMessage(MarketplaceAlias marketplace) : base(marketplace)
        {
        }

        public string Marketplace { get; set; }
    }
}
