using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Services;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Marketplace.Application.Catalog;

namespace Yah.Hub.Marketplace.Application.Authentication
{
    public abstract class AbstractAuthenticationService : AbstractMarketplaceService, IAuthenticationService
    {

        private IAccountConfigurationService AccountConfigurationService;

        public AbstractAuthenticationService(ILogger<AbstractAuthenticationService> logger, IConfiguration configurationService, IAccountConfigurationService accountConfigurationService) : base(configurationService, logger)
        {
            this.AccountConfigurationService = accountConfigurationService;
        }

        #region abstract

        protected abstract Task<MarketplaceServiceMessage<AccountConfiguration>> AuthenticateMarketplace(MarketplaceServiceMessage<string> serviceMessage);
        protected abstract Task<MarketplaceServiceMessage<AccountConfiguration>> RefreshMarketplaceToken(MarketplaceServiceMessage serviceMessage);
        protected abstract Task<MarketplaceServiceMessage<string>> GetMarketplaceAuthorizationUrl(MarketplaceServiceMessage serviceMessage);
        protected abstract Task<MarketplaceServiceMessage> ValiadateAuthorization(MarketplaceServiceMessage serviceMessage);

        #endregion

        #region [Credentials Methods]
        public virtual async Task<ServiceMessage> ValidateCredentials(ServiceMessage serviceMessage)
        {
            #region [Code]
            try
            {
                serviceMessage.Identity.IsValidVendorTenantAccountIdentity();

                var configResult = await this.GetAccountConfiguration(serviceMessage);

                var credentialResult = await this.ValiadateAuthorization(serviceMessage.AsMarketplaceServiceMessage(serviceMessage.Identity, configResult.Data));

                if (!credentialResult.IsValid)
                {
                    return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, credentialResult.Errors);
                }

                return ServiceMessage.CreateValidResult(serviceMessage.Identity);
            }
            catch (Exception ex)
            {
                Error error = new Error(ex);
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                return ServiceMessage.CreateInvalidResult(serviceMessage.Identity, new Error(ex));
            }
            #endregion
        }

        public virtual async Task<ServiceMessage> SetAuthentication(ServiceMessage<string> serviceMessage)
        {
            #region Code

            // result
            var marketplaceMessage = new ServiceMessage(serviceMessage.Identity);

            try
            {
                // configuration
                var configResult = await this.GetAccountConfiguration(serviceMessage);

                if (!configResult.IsValid)
                {
                    marketplaceMessage.WithErrors(configResult.Errors);
                    return marketplaceMessage;
                }

                // marketplace service message
                var marketplaceServiceMessage = new MarketplaceServiceMessage<string>(serviceMessage.Identity, configResult.Data, serviceMessage.Data);

                var authResult = await this.AuthenticateMarketplace(marketplaceServiceMessage);

                if (!authResult.IsValid)
                    return authResult;

                var setConfigResult = await this.SaveAccountConfiguration(new ServiceMessage<AccountConfiguration>(authResult.Identity, authResult.Data));

                if (!setConfigResult.IsValid)
                    marketplaceMessage.WithErrors(setConfigResult.Errors);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Erro ao autenticar");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                marketplaceMessage.WithError(error);
            }

            return marketplaceMessage;

            #endregion
        }

        public virtual async Task<ServiceMessage> RenewToken(ServiceMessage authMessage)
        {
            #region Code

            // result
            var marketplaceMessage = new ServiceMessage(authMessage.Identity);

            try
            {
                // configuration
                var configResult = await this.GetAccountConfiguration(authMessage);
                if (!configResult.IsValid)
                {
                    marketplaceMessage.WithErrors(configResult.Errors);
                    return marketplaceMessage;
                }

                // set config
                var marketplaceServiceMessage = new MarketplaceServiceMessage(authMessage.Identity, configResult.Data);

                var authResult = await this.RefreshMarketplaceToken(marketplaceServiceMessage);

                if (!authResult.IsValid)
                    return authResult;

                var setConfigResult = await this.SaveAccountConfiguration(new ServiceMessage<AccountConfiguration>(authMessage.Identity, authResult.Data));

                if (!setConfigResult.IsValid)
                    marketplaceMessage.WithErrors(setConfigResult.Errors);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Erro atualizar token");
                Logger.LogCustomCritical(error, authMessage.Identity);
                marketplaceMessage.WithError(error);
            }

            return marketplaceMessage;

            #endregion
        }

        public async virtual Task<ServiceMessage<string>> GetAuthorizationUrl(ServiceMessage authMessage)
        {
            #region Code

            // result
            var marketplaceMessage = new ServiceMessage<string>(authMessage.Identity);

            try
            {
                // client must have account previously
                var configResult = await this.GetAccountConfiguration(authMessage);
                if (!configResult.IsValid)
                {
                    marketplaceMessage.WithErrors(configResult.Errors);
                    return marketplaceMessage;
                }

                var marketplaceServiceMessage = new MarketplaceServiceMessage(authMessage.Identity, configResult.Data);

                var authResult = await this.GetMarketplaceAuthorizationUrl(marketplaceServiceMessage);

                if (!authResult.IsValid)
                {
                    marketplaceMessage.WithErrors(authResult.Errors);
                    return marketplaceMessage;
                }
                    
                marketplaceMessage.WithData(authResult.Data);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while get auth uri");
                Logger.LogCustomCritical(error, authMessage.Identity);
                marketplaceMessage.WithError(error);
            }

            return marketplaceMessage;

            #endregion
        }

        #endregion

        #region Account Configuration Methods

        public async Task<ServiceMessage<AccountConfiguration>> GetAccountConfiguration(ServiceMessage serviceMessage)
        {
            var result = new ServiceMessage<AccountConfiguration>(serviceMessage.Identity);

            try
            {
                var configResult = await this.AccountConfigurationService.GetConfiguration(serviceMessage.AsMarketplaceServiceMessage(serviceMessage.Identity, GetMarketplace()));
                if (configResult.IsValid)
                    result.WithData(configResult.Data);
                else
                    result.WithErrors(configResult.Errors);
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while get configuration");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }
            return result;
        }

        public async Task<ServiceMessage<AccountConfiguration>> SaveAccountConfiguration(ServiceMessage<AccountConfiguration> configuration)
        {
            var result = new ServiceMessage<AccountConfiguration>(configuration.Identity);
            try
            {
                var configResult = await this.AccountConfigurationService.SetConfiguration(configuration);
                if (!configResult.IsValid)
                    result.WithErrors(configResult.Errors);
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while update configuration");
                Logger.LogCustomCritical(error, configuration.Identity);
                result.WithError(error);
            }

            return result;
        }

        #endregion
    }
}

