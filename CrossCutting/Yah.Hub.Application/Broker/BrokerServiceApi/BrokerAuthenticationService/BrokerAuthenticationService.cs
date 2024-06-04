using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerAuthenticationService
{
    public class BrokerAuthenticationService : AbstractService, IBrokerAuthenticationService
    {
        #region [Properties]
        private IAccountConfigurationService ConfigurationService { get; set; }
        private IBrokenClient Client { get; }
        #endregion

        public BrokerAuthenticationService(IConfiguration configuration, ILogger<BrokerAuthenticationService> logger, IBrokenClient client, IAccountConfigurationService configurationService) : base(configuration, logger)
        {
            ConfigurationService = configurationService;
            Client = client;
        }

        public async Task<ServiceMessage> SaveAccountConfiguration(MarketplaceServiceMessage<Dictionary<string,string>> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            var configuration = await ConfigurationService.GetConfiguration(message);

            if (configuration.Data == null || !configuration.IsValid)
            {
                if (configuration.Errors.Any())
                {
                    result.WithErrors(configuration.Errors);
                }
                else
                {
                    result.WithError(new Error("Não foi possível validar as credenciais informadas", "", ErrorType.Business));
                }

                return result;
            }

            var resultRequest = await this.Client.SaveAccountConfigurationSync(message);

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            return result;

            #endregion
        }

        public async Task<ServiceMessage> ValidadeCredentials(MarketplaceServiceMessage message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var configuration = await ConfigurationService.GetConfiguration(message);

            if (configuration.Data == null || !configuration.IsValid)
            {
                if (configuration.Errors.Any())
                {
                    result.WithErrors(configuration.Errors);
                }
                else
                {
                    result.WithError(new Error("Não foi possível validar as credenciais informadas", "", ErrorType.Business));
                }

                return result;
            }

            var resultRequest = await this.Client.ValidadeCredentials(message);

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            return result;
            #endregion
        }
    }
}
