using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Authentication
{
    public class ViaVarejoAuthenticationService : AbstractAuthenticationService, IAuthenticationService
    {
        private IViaVarejoClient Client { get; }

        public ViaVarejoAuthenticationService(
            ILogger<AbstractAuthenticationService> logger,
            IConfiguration configurationService,
            IAccountConfigurationService accountConfigurationService,
            IViaVarejoClient viaVarejoClient) 
            : base(logger, configurationService, accountConfigurationService)
        {
            Client = viaVarejoClient;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.ViaVarejo;
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

        protected async override Task<MarketplaceServiceMessage> ValiadateAuthorization(MarketplaceServiceMessage serviceMessage)
        {
            return await this.Client.ValidateCredentials(serviceMessage);
        }
    }
}
