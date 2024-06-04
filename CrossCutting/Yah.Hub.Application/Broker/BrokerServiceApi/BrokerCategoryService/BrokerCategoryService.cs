using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCategoryService
{
    public class BrokerCategoryService : AbstractService, IBrokerCategoryService
    {
        private IAccountConfigurationService ConfigurationService { get; set; }
        private IBrokenClient Client { get; }

        public BrokerCategoryService(IConfiguration configuration, ILogger<BrokerCategoryService> logger, IBrokenClient client, IAccountConfigurationService configurationService) : base(configuration, logger)
        {
            ConfigurationService = configurationService;
            Client = client;
        }

        public async Task<ServiceMessage<List<MarketplaceCategory>>> GetCategory(MarketplaceServiceMessage<string> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<MarketplaceCategory>>(message.Identity);

            if (String.IsNullOrEmpty(message.Data))
            {
                result.WithError(new Error("Necessário informar um Id para a busca da categoria no marketplace", "", ErrorType.Business));
                return result;
            }

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

            var resultRequest = await this.Client.GetCategoryById(message.Data.AsMarketplaceServiceMessage(message.Identity, configuration.Data));

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
                return result;
            }

            result.WithData(resultRequest.Data.Data);

            return result;
            #endregion
        }

        public async Task<ServiceMessage<List<MarketplaceCategory>>> GetCategories(MarketplaceServiceMessage message)
        {
            #region [Code]
            var result = new ServiceMessage<List<MarketplaceCategory>>(message.Identity);

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

            var resultRequest = await this.Client.GetCategories(message);

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
                return result;
            }

            result.WithData(resultRequest.Data.Data);

            return result;
            #endregion
        }

        public async Task<ServiceMessage<List<MarketplaceAttributes>>> GetAttribute(MarketplaceServiceMessage<string?> message)
        {
            #region [Code]
            var result = new ServiceMessage<List<MarketplaceAttributes>>(message.Identity);

            if (String.IsNullOrEmpty(message.Data))
            {
                result.WithError(new Error("Necessário informar um Id para a busca do atributo no marketplace", "", ErrorType.Business));
                return result;
            }

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

            var resultRequest = await this.Client.GetAttributes(message.Data.AsMarketplaceServiceMessage(message.Identity, configuration.Data));

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
                return result;
            }

            result.WithData(resultRequest.Data.Data);

            return result;
            #endregion
        }
    }
}
