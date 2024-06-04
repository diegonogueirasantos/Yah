using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCatalogService
{
    public class BrokerCatalogService : AbstractService, IBrokerCatalogService
    {
        #region [Constant]

        private Dictionary<Type, string> EnqueueCommandTypes = new Dictionary<Type, string>() 
        {
            {typeof(Product), "EnqueueProductCommand"},
            {typeof(ProductPrice), "EnqueueProductPriceCommand" },
            {typeof(ProductInventory),  "EnqueueProductInventoryCommand"},
            {typeof(Announcement), "EnqueueAnnouncementCommand"},
            {typeof(AnnouncementPrice),  "EnqueueAnnouncementPriceCommand"},
            {typeof(AnnouncementInventory), "EnqueueAnnouncementInventoryCommand" }
        };
        #endregion

        #region [Properties]
        private IAccountConfigurationService ConfigurationService { get; }
        private IBrokenClient Client { get; }
        #endregion

        #region [Construct]
        public BrokerCatalogService(IConfiguration configuration, ILogger<BrokerCatalogService> logger, IBrokenClient client, IAccountConfigurationService configurationService) : base(configuration, logger)
        {
            Client = client;
            ConfigurationService = configurationService;
        }
        #endregion

        #region [Methods]

        public async Task<ServiceMessage> RequestAsync<T>(MarketplaceServiceMessage<(T RequestData, Operation OperationId)> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            var command = new CommandMessage<T>(message.Identity)
            {
                Data = message.Data.RequestData,
                Marketplace = message.Marketplace,
                ServiceOperation = message.Data.OperationId,
                CorrelationId = Guid.NewGuid().ToString(),
                EventDateTime = DateTimeOffset.UtcNow
            };

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

            var resultRequest = await Client.RequestMessage(new WrapperRequest<CommandMessage<T>>(command, HttpMethod.Post, EnqueueCommandTypes.TryGetValueOrDefault(typeof(T))).AsMarketplaceServiceMessage(message.Identity, configuration.Data));

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            return result;

            #endregion
        }
        #endregion

    }
}