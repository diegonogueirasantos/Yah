using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Netshoes.Application.Client;

namespace Yah.Hub.Marketplace.Netshoes.Application.Authentication
{
    public class NetshoesAuthenticationService : AbstractAuthenticationService, IAuthenticationService
    {
        private INetshoesClient Client { get; }

        public NetshoesAuthenticationService(
            ILogger<AbstractAuthenticationService> logger,
            IConfiguration configurationService,
            IAccountConfigurationService accountConfigurationService,
            INetshoesClient netshoesClient) 
            : base(logger, configurationService, accountConfigurationService)
        {
            Client = netshoesClient;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Netshoes;
        }

        protected override Task<MarketplaceServiceMessage<AccountConfiguration>> AuthenticateMarketplace(MarketplaceServiceMessage<string> serviceMessage)
        {
            throw new NotImplementedException();
        }

        protected override Task<MarketplaceServiceMessage<string>> GetMarketplaceAuthorizationUrl(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        protected override Task<MarketplaceServiceMessage<AccountConfiguration>> RefreshMarketplaceToken(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        protected override async Task<MarketplaceServiceMessage> ValiadateAuthorization(MarketplaceServiceMessage serviceMessage)
        {
            return await this.Client.TryAuthenticate(serviceMessage);
        }
    }
}
