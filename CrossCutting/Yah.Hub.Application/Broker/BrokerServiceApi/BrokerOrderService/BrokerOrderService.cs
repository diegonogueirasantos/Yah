using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerOrderService
{
    public class BrokerOrderService : AbstractService, IBrokerOrderService
    {
        #region [Properties]
        private IAccountConfigurationService ConfigurationService { get; }
        private IBrokenClient Client { get; }
        #endregion

        private Dictionary<Type, string> RequestOrderType = new Dictionary<Type, string>()
        {
            {typeof(OrderStatusInvoice), "TryInvoiceOrderAsync"},
            {typeof(OrderStatusCancel), "TryCancelOrderAsync" },
            {typeof(OrderStatusShipment),  "TryShipOrderAsync"},
            {typeof(OrderStatusDelivered), "TryDeliveryOrderAsync"}
        };

        public BrokerOrderService(IConfiguration configuration, ILogger<BrokerOrderService> logger, IBrokenClient client, IAccountConfigurationService configurationService) : base(configuration, logger)
        {
            ConfigurationService = configurationService;
            Client = client;
        }

        public async Task<ServiceMessage> UpdateOrder<T>(MarketplaceServiceMessage<T> message)
        {
            #region [Code]
            var result = new ServiceMessage(message.Identity);

            var configuration = await ConfigurationService.GetConfiguration(message);

            if(configuration.Data == null || !configuration.IsValid)
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

            var resultRequest = await Client.RequestMessage(new WrapperRequest<T>(message.Data, HttpMethod.Put, RequestOrderType.TryGetValueOrDefault(typeof(T))).AsMarketplaceServiceMessage(message.Identity, configuration.Data));

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            return result;
            #endregion
        }

        public async Task<ServiceMessage<ShipmentLabel>> GetShipmentLabel(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]
            var result = new ServiceMessage<ShipmentLabel>(message.Identity);

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

            var resultRequest = await Client.GetShipmentLabel(message.Data.AsMarketplaceServiceMessage(message.Identity, configuration.Data));

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
            }

            if(resultRequest.Data != null)
            {
                result.WithData(resultRequest.Data.Data);
            }

            return result;
            #endregion
        }
    }
}
