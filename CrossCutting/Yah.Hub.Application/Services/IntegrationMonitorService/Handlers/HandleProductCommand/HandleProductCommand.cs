using System;
using Amazon.Auth.AccessControlPolicy;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.IntegrationMonitorRepository;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
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
		public async Task<ServiceMessage> HandleEntityState(CommandMessage<Product> serviceMessage)
        {
            var result = new ServiceMessage(serviceMessage.Identity);

            try
            {
                var newState = new MarketplaceEntityState(serviceMessage.Data.Id, serviceMessage.Data.Id, serviceMessage.EventDateTime, serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Marketplace)
                {
                    ProductInfo = new ProductIntegrationInfo(serviceMessage.Data.Id, serviceMessage.Data.Id, Common.Enums.EntityStatus.Unknown, serviceMessage.EventDateTime) { Name = serviceMessage.Data.Name, Description = serviceMessage.Data.Description }
                };

                var currentState = await this.GetBtRefferId(newState.ReferenceId.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.Marketplace));

                if (!currentState.Data.Any())
                {
                    if (serviceMessage.IsAnnouncement)
                        return result;

                    var mergedState = MergeEntityStates(serviceMessage, null);
                    await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, newState));
                }
                else
                {
                    foreach (var oldState in currentState.Data)
                    {
                        newState.Id = oldState.Id ?? newState.Id;
                        newState.ReferenceId = oldState.ReferenceId ?? newState.Id;

                        if (oldState.DateTime > newState.DateTime)
                            continue;

                        //if (serviceMessage.Data.ServiceOperation == Common.Enums.Operation.Delete && !serviceMessage.Data.Data.Errors.Any())
                        //    await this.IntegrationMonitorRepository.DeleteAsync(oldState.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                        var finalState = MergeEntityStates(serviceMessage, oldState.ProductInfo);

                        newState.ProductInfo = finalState;

                        if (finalState != null)
                            await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, newState));

                        //if (!saveResult.IsValid)
                        //    result.WithErrors(saveResult.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        public ProductIntegrationInfo MergeEntityStates(CommandMessage<Product> source, ProductIntegrationInfo? oldState)
        {
            var finalState = oldState ?? new ProductIntegrationInfo(source.Data.Id, source.Data.IntegrationId, Common.Enums.EntityStatus.Unknown, source.EventDateTime) { Name = source.Data.Name, Description = source.Data.Description };

            if (oldState == null && source.ServiceOperation != Common.Enums.Operation.Delete)
                finalState.SubStatus =  Common.Enums.EntitySubstatus.Creating;

            if(oldState != null)
                finalState.SubStatus = Common.Enums.EntitySubstatus.Updating;

            return finalState;
        }
    }
}

