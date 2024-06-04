using System;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Common.Marketplace
{
    public class MarketplaceServiceMessage : ServiceMessage.ServiceMessage, IMarketplaceServiceMessage
    {
        public AccountConfiguration AccountConfiguration { get; private set; }
        public MarketplaceAlias Marketplace { get; private set; }

        public MarketplaceServiceMessage(Identity.Identity identity, AccountConfiguration accountConfiguration) : base(identity)
        {
            if(accountConfiguration?.Marketplace != null)
            {
                Marketplace = accountConfiguration.Marketplace;
                AccountConfiguration = accountConfiguration;
            }
            else
            {

            }
        }

        public MarketplaceServiceMessage(Identity.Identity identity, MarketplaceAlias marketplace) : base(identity)
        {
            this.Marketplace = marketplace;
        }

    }

    public class MarketplaceServiceMessage<T> : MarketplaceServiceMessage, IMarketplaceServiceMessage<T>
    {
        public T Data { get; set; }

        public MarketplaceServiceMessage(Identity.Identity identity, AccountConfiguration accountConfiguration, T data) : base(identity, accountConfiguration)
        {
            this.Data = data;
        }

        public MarketplaceServiceMessage(Identity.Identity identity, MarketplaceAlias marketplace, T data) : base(identity, marketplace)
        {
            this.Data = data;
        }

        public MarketplaceServiceMessage(Identity.Identity identity, AccountConfiguration accountConfiguration) : base(identity, accountConfiguration)
        {
        }

        public void WithData(T data)
        {
            this.Data = data;
        }
    }
}

