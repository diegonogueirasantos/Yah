using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.ShipmentLabel;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerMonitorService
{
    public class BrokerMonitorService : AbstractService, IBrokerMonitorService
    {
        #region [Properties]
        private IAccountConfigurationService ConfigurationService { get; }
        private IBrokenClient Client { get; }
        #endregion

        #region [Construct]
        public BrokerMonitorService(IConfiguration configuration, ILogger<BrokerMonitorService> logger, IAccountConfigurationService accountConfigurationService, IBrokenClient brokenClient) : base(configuration, logger)
        {
            ConfigurationService = accountConfigurationService;
            Client = brokenClient;
        }
        #endregion

        #region [Methods]
        public async Task<ServiceMessage> MonitorProductStatus(MarketplaceServiceMessage message)
        {
           return await Client.MonitorProductStatus(message);
        }

        public async Task<ServiceMessage> ConsumeCommands(MarketplaceServiceMessage message)
        {
            return await Client.ConsumeCommands(message);
        }

        public async Task<ServiceMessage<List<IntegrationSummary>>> GetIntegrationSummary(MarketplaceServiceMessage message)
        {
            #region [Code]
            var result = new ServiceMessage<List<IntegrationSummary>>(message.Identity);

            var resultRequest = await Client.GetIntegrationSummary(message);

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            if (resultRequest.Data != null)
            {
                result.WithData(resultRequest.Data.Data);
            }

            return result;

            #endregion
        }

        public async Task<ServiceMessage<EntityStateSearchResult>> GetIntegrationByStatus(MarketplaceServiceMessage<EntityStateQuery> message)
        {
            #region [Code]
            var result = new ServiceMessage<EntityStateSearchResult>(message.Identity);

            var resultRequest = await Client.GetIntegrationByStatus(message);

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            if (resultRequest.Data != null)
            {
                result.WithData(resultRequest.Data.Data);
            }

            return result;

            #endregion
        }
        #endregion

    }
}
