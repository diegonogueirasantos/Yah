using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Services.IntegrationMonitorService
{
    public partial class IntegrationMonitorService : IIntegrationMonitorService
    {
        /// <summary>
        /// TODO: REMOVER PARTIAL E MUDAR PARA CAMADA APLICACIONAL DE MARKETPLACE
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
		public async Task<ServiceMessage> HandleEntityState(CommandMessage<Announcement> serviceMessage)
        {
            var result = new ServiceMessage(serviceMessage.Identity);

            try
            {
                var newState = new MarketplaceEntityState(serviceMessage.Data.Id, serviceMessage.Data.Product.Id ?? serviceMessage.Data.Product.IntegrationId, serviceMessage.EventDateTime, serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Marketplace)
                {
                    ProductInfo = new ProductIntegrationInfo(serviceMessage.Data.Id, serviceMessage.Data.Product.Id ?? serviceMessage.Data.Product.IntegrationId, Common.Enums.EntityStatus.Unknown, serviceMessage.EventDateTime) { Name = serviceMessage.Data.Item.Title, Description = serviceMessage.Data.Product.Description }
                };

                var currentState = await this.IntegrationMonitorRepository.GetAsync(newState.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.Marketplace));

                if (currentState.Data == null)
                {
                    var mergedState = MergeEntityStates(serviceMessage, null);
                    newState.ProductInfo = mergedState;
                    await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, newState));
                }
                else
                {
                    newState.Id = currentState.Data.Id ?? newState.Id;
                    newState.ReferenceId = currentState.Data.ReferenceId ?? newState.Id;

                    if (currentState.Data.DateTime > newState.DateTime)
                        return result;

                    var finalState = MergeEntityStates(serviceMessage, currentState.Data.ProductInfo);

                    newState.ProductInfo = finalState;

                    if (finalState != null)
                        await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, newState));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        public ProductIntegrationInfo MergeEntityStates(CommandMessage<Announcement> source, ProductIntegrationInfo? oldState)
        {
            var finalState = oldState ?? new ProductIntegrationInfo(source.Data.Id, source.Data.Product.Id ?? source.Data.Product.IntegrationId, Common.Enums.EntityStatus.Unknown, source.EventDateTime) { Name = source.Data.Item.Title, Description = source.Data.Product.Description  };

            if (oldState == null && source.ServiceOperation != Common.Enums.Operation.Delete)
                finalState.SubStatus = Common.Enums.EntitySubstatus.Creating;

            if (oldState != null)
                finalState.SubStatus = Common.Enums.EntitySubstatus.Updating;
            else
                finalState.Status = Common.Enums.EntityStatus.Waiting;

            return finalState;
        }
    }
}

