using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.B2W.Application.Client.Interface;
using Yah.Hub.Marketplace.B2W.Application.Models;

namespace Yah.Hub.Marketplace.B2W.Application.Authentication
{
    public class B2WAuthenticationService : AbstractAuthenticationService, IAuthenticationService
    {
        private readonly IB2WClient Client;

        public B2WAuthenticationService(ILogger<AbstractAuthenticationService> logger, IConfiguration configurationService, IAccountConfigurationService accountConfigurationService, IB2WClient client) : base(logger, configurationService, accountConfigurationService)
        {
            this.Client = client;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.B2W;
        }

        protected override Task<MarketplaceServiceMessage<AccountConfiguration>> AuthenticateMarketplace(MarketplaceServiceMessage<string> serviceMessage)
        {
            throw new NotImplementedException();
        }

        protected override Task<MarketplaceServiceMessage<string>> GetMarketplaceAuthorizationUrl(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        protected override async Task<MarketplaceServiceMessage<AccountConfiguration>> RefreshMarketplaceToken(MarketplaceServiceMessage serviceMessage)
        {
            var result = new MarketplaceServiceMessage<AccountConfiguration>(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            var auth = new Auth() { accessKey = serviceMessage.AccountConfiguration.AccessToken, email = serviceMessage.AccountConfiguration.Email };

            var tokenResult = await this.Client.RenewRehubToken(auth.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

            if (!tokenResult.IsValid)
            {
                result.WithErrors(tokenResult.Errors);
                return result;
            }

            result.WithData(serviceMessage.AccountConfiguration);
            result.Data.RefreshToken = tokenResult.Data.token;
            
            return result;
        }

        protected override Task<MarketplaceServiceMessage> ValiadateAuthorization(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }
    }
}
