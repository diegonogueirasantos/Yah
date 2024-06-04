using System;
using Microsoft.Extensions.Configuration;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.MercadoLivre.Application.Client;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Authorization
{
    public class MercadoLivreAuthenticationService : AbstractAuthenticationService, IAuthenticationService
    {
        private readonly IMercadoLivreClient Client;

        public MercadoLivreAuthenticationService(ILogger<MercadoLivreAuthenticationService> logger, IConfiguration configuration, IAccountConfigurationService accountConfigurationService, IMercadoLivreClient client)
            : base(logger, configuration, accountConfigurationService)
        {

            this.Client = client;
        }

        protected override async Task<MarketplaceServiceMessage<AccountConfiguration>> AuthenticateMarketplace(MarketplaceServiceMessage<string> serviceMessage)
        {
            var result = new MarketplaceServiceMessage<AccountConfiguration>(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            var tokenResult = await this.Client.GetAccessToken(serviceMessage);
            if (!tokenResult.IsValid)
            {
                result.WithErrors(tokenResult.Errors);
                return result;
            }

            result.WithData(serviceMessage.AccountConfiguration);
            result.Data.RefreshToken = tokenResult.Data.RefreshToken ?? serviceMessage.Data;
            result.Data.AccessToken = tokenResult.Data.AccessToken;
            result.Data.AppId = tokenResult.Data.UserId;


            return result;
        }

        protected override async Task<MarketplaceServiceMessage<AccountConfiguration>> RefreshMarketplaceToken(MarketplaceServiceMessage serviceMessage)
        {
            var result = new MarketplaceServiceMessage<AccountConfiguration>(serviceMessage.Identity, serviceMessage.AccountConfiguration);


            var tokenResult = await this.Client.RefreshToken(serviceMessage);
            if (!tokenResult.IsValid)
            {
                result.WithErrors(tokenResult.Errors);
                return result;
            }

            result.WithData(serviceMessage.AccountConfiguration);
            result.Data.RefreshToken = tokenResult.Data.RefreshToken;
            result.Data.AccessToken = tokenResult.Data.AccessToken;


            return result;
        }

        protected override async Task<MarketplaceServiceMessage<string>> GetMarketplaceAuthorizationUrl(MarketplaceServiceMessage serviceMessage)
        {
            return await this.Client.GetAuthorizationUrl(serviceMessage);
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }

        protected override async Task<MarketplaceServiceMessage> ValiadateAuthorization(MarketplaceServiceMessage serviceMessage)
        {
            var result = await this.Client.GetOrder("0000".AsMarketplaceServiceMessage(serviceMessage));

            if (result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                serviceMessage.WithError(new Error("Forbidden", "Invalida Token", ErrorType.Authentication));

            return serviceMessage;
        }
    }
}

