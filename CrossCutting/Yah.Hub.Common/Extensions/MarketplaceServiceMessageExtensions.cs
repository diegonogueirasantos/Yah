using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;

namespace Yah.Hub.Common.Extensions
{
    public static class MarketplaceServiceMessageExtensions
    {
        public static MarketplaceServiceMessage<T> AsMarketplaceServiceMessage<T>(this T model, Identity.Identity identity, AccountConfiguration accountConfiguration)
        {
            return new MarketplaceServiceMessage<T>(identity, accountConfiguration, model);
        }

        public static MarketplaceServiceMessage<T> AsMarketplaceServiceMessage<T>(this T model, MarketplaceServiceMessage marketplaceServiceMessage)
        {
            return marketplaceServiceMessage.AccountConfiguration == null ? new MarketplaceServiceMessage<T>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.Marketplace, model) : new MarketplaceServiceMessage<T>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration, model);
        }

        public static MarketplaceServiceMessage<T> AsMarketplaceServiceMessage<T>(this T model, Identity.Identity identity, MarketplaceAlias marketplace)
        {
            return new MarketplaceServiceMessage<T>(identity, marketplace, model);
        }

        public static MarketplaceServiceMessage AsMarketplaceServiceMessage(this Identity.Identity identity, AccountConfiguration accountConfiguration)
        {
            return new MarketplaceServiceMessage(identity, accountConfiguration);
        }

        public static MarketplaceServiceMessage AsMarketplaceServiceMessage(this Identity.Identity identity, MarketplaceAlias marketplace)
        {
            return new MarketplaceServiceMessage(identity, marketplace);
        }
    }
}
