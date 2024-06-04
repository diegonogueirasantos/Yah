using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Magalu.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Models;

namespace Yah.Hub.Marketplace.Magalu.Application.Authentication
{
    public class MagaluAuthenticationService : AbstractAuthenticationService, IAuthenticationService
    {
        private readonly IMagaluClient Client;
        private readonly ISecurityService Security;

        public MagaluAuthenticationService(
            ILogger<AbstractAuthenticationService> logger,
            IConfiguration configurationService,
            IAccountConfigurationService accountConfigurationService,
            IMagaluClient Client,
            ISecurityService security) 
            : base(logger, configurationService, accountConfigurationService)
        {
            this.Client = Client;
            this.Security = security;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Magalu;
        }

        protected async override Task<MarketplaceServiceMessage<AccountConfiguration>> AuthenticateMarketplace(MarketplaceServiceMessage<string> serviceMessage)
        {
            #region [Code]
            var tenantId = serviceMessage.AccountConfiguration.TenantId;
            var vendorId = serviceMessage.AccountConfiguration.VendorId;
            var accountId = serviceMessage.AccountConfiguration.AccountId;

            var isMigration = serviceMessage.Identity.IsValidUsernameIdentity();

            var result = new MarketplaceServiceMessage<AccountConfiguration>(this.Security.ImpersonateClaimIdentity(serviceMessage.Identity, vendorId, tenantId, accountId).Result, serviceMessage.AccountConfiguration, serviceMessage.AccountConfiguration);

            var requestToken = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" } ,
                { "redirect_uri", $"{Configuration["Platform:RedirectUri"]}" },
                {"client_id", $"{Configuration["Marketplace:ClientId"]}" },
                {"client_secret", $"{Configuration["Marketplace:ClientSecret"]}" },
                {"code", $"{serviceMessage.Data}" }
            };

            var requestResult = await this.Client.AuthenticationTokenAsync(requestToken.AsMarketplaceServiceMessage(serviceMessage));

            if (!requestResult.IsValid)
            {
                if (requestResult.Errors == null)
                {
                    result.WithError(new Error($"Erro ao tentar obter o access_token no marketplace Magazine Luiza",$"Erro desconhecido. Tenant:{serviceMessage.Identity.GetTenantId()} Vendor: {serviceMessage.Identity.GetVendorId()} Account: {serviceMessage.Identity.GetAccountId()}",ErrorType.Technical));
                }
                else
                {
                    result.WithErrors(requestResult.Errors);
                }

                return result;
            }
            else
            {
                result.Data.RefreshToken = requestResult.Data.RefreshToken ?? serviceMessage.AccountConfiguration.RefreshToken;
                result.Data.AccessToken = requestResult.Data.AccessToken ?? serviceMessage.AccountConfiguration.AccessToken;
            }


            if(requestResult.IsValid && isMigration)
            {
                var finalizedRequest = await this.Client.FinalizedAuthenticationAsync(serviceMessage);

                if (!finalizedRequest.IsValid)
                {
                    if (finalizedRequest.Errors == null)
                    {
                        result.WithError(new Error($"Erro ao tentar finalizar processo de rollout Token no marketplace Magazine Luiza", $"Erro desconhecido. Tenant:{serviceMessage.Identity.GetTenantId()} Vendor: {serviceMessage.Identity.GetVendorId()} Account: {serviceMessage.Identity.GetAccountId()}", ErrorType.Technical));
                    }
                    else
                    {
                        result.WithErrors(requestResult.Errors);
                    }

                    return result;
                }
            }

            return result;
            #endregion
        }

        protected override Task<MarketplaceServiceMessage<string>> GetMarketplaceAuthorizationUrl(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        protected async override Task<MarketplaceServiceMessage<AccountConfiguration>> RefreshMarketplaceToken(MarketplaceServiceMessage serviceMessage)
        {
            #region [Code]
            var result = new MarketplaceServiceMessage<AccountConfiguration>(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            var requestToken = new Dictionary<string, string>()
            {
                { "grant_type", "refresh_token" } ,
                { "redirect_uri", $"{Configuration["Platform:RedirectUri"]}" },
                { "client_id", $"{Configuration["Marketplace:ClientId"]}" },
                { "client_secret", $"{Configuration["Marketplace:ClientSecret"]}" },
                { "refresh_token", $"{serviceMessage.AccountConfiguration.RefreshToken}" }
            };

            var requestResult = await this.Client.AuthenticationTokenAsync(requestToken.AsMarketplaceServiceMessage(serviceMessage));

            if (!requestResult.IsValid)
            {
                if (requestResult.Errors == null)
                {
                    result.WithError(new Error($"Erro ao tentar obter o access_token no marketplace Magazine Luiza", $"Erro desconhecido. Tenant:{serviceMessage.Identity.GetTenantId()} Vendor: {serviceMessage.Identity.GetVendorId()} Account: {serviceMessage.Identity.GetAccountId()}", ErrorType.Technical));
                }
                else
                {
                    result.WithErrors(requestResult.Errors);
                }

                return result;
            }
            result.WithData(serviceMessage.AccountConfiguration);
            result.Data.RefreshToken = requestResult.Data.RefreshToken ?? serviceMessage.AccountConfiguration.RefreshToken;
            result.Data.AccessToken = requestResult.Data.AccessToken ?? serviceMessage.AccountConfiguration.AccessToken;

            return result;
            #endregion
        }

        protected override async Task<MarketplaceServiceMessage> ValiadateAuthorization(MarketplaceServiceMessage serviceMessage)
        {
            return await this.Client.ValidateAuthorization(serviceMessage);
        }
    }
}
