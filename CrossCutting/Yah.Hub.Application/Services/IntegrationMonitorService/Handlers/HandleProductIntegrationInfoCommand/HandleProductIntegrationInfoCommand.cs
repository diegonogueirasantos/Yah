using System;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
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
		public async Task<ServiceMessage> HandleEntityState(CommandMessage<ProductIntegrationInfo> serviceMessage)
        {
            var result = new ServiceMessage(serviceMessage.Identity);

            var states = new List<MarketplaceEntityState>();

            try
            {
                var newState = new MarketplaceEntityState(serviceMessage.Data.Id, serviceMessage.Data.ReferenceId, serviceMessage.Data.DateTime, serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Marketplace)
                {
                    ProductInfo = serviceMessage.Data
                };


                if (serviceMessage.Data.Id == serviceMessage.Data.ReferenceId)
                {
                    var currentState = await this.GetBtRefferId(serviceMessage.Data.ReferenceId.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.Marketplace));
                    states = currentState.Data;
                }
                else
                {
                    var currentState = await this.IntegrationMonitorRepository.GetAsync(serviceMessage.Data.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.Marketplace));
                    if(currentState.Data != null)
                        states.Add(currentState.Data);
                }

                if (!states.Any())
                {
                    var mergedState = MergeEntityStates(newState.ProductInfo, null);
                    await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, newState));
                }
                else
                {
                    foreach (var oldState in states)
                    {
                        newState.Id = oldState.Id ?? newState.Id;
                        newState.ReferenceId = oldState.ReferenceId ?? newState.Id;

                        if (oldState.DateTime > newState.DateTime)
                            continue;

                        if (serviceMessage.ServiceOperation == Common.Enums.Operation.Delete && !serviceMessage.Data.Errors.Any())
                        {
                            await this.IntegrationMonitorRepository.DeleteAsync(oldState.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.Marketplace));
                            continue;
                        }

                        var mergedState = MergeEntityStates(newState.ProductInfo, oldState.ProductInfo);

                        newState.ProductInfo = mergedState;

                        var saveResult = await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, newState));

                        if (!saveResult.IsValid)
                            result.WithErrors(saveResult.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        public ProductIntegrationInfo MergeEntityStates(ProductIntegrationInfo newState, ProductIntegrationInfo? oldState)
        {
            var finalState = oldState ?? newState;

            // unknow
            if (newState.Status == Common.Enums.EntityStatus.Unknown)
            {
                if (oldState == null)
                {
                    // set product to initial state
                    finalState.Status = Common.Enums.EntityStatus.Waiting;
                    finalState.SubStatus = EntitySubstatus.Creating;

                    if (newState.Errors.Any())
                    {
                        finalState.Status = EntityStatus.Declined;
                        finalState.Errors = newState.Errors;
                    }
                    
                }
                else
                {
                    finalState.Status = oldState.Status;
                    finalState.SubStatus = EntitySubstatus.Creating;

                    if (newState.Errors.Any())
                    {
                        finalState.Status = oldState.Status != EntityStatus.Accepted ? EntityStatus.Declined : oldState.Status;
                        finalState.Errors = newState.Errors;
                    }
                }
                //merge SKUs
                if (!newState.Skus?.Any() ?? true)
                    finalState.Skus = oldState?.Skus ?? new List<SkuIntegrationInfo>();

                finalState.Name = oldState?.Name ?? newState.Name;
                finalState.SubStatus = EntitySubstatus.Synced;

                return finalState;
            }

            // accepted
            if (oldState?.Status == Common.Enums.EntityStatus.Accepted)
            {
                if (newState.Errors.Any())
                {
                    finalState.Status = oldState.Status;
                    finalState.SubStatus = EntitySubstatus.Synced;
                }
            }
            else
            {
                if (newState.Errors.Any())
                {
                    finalState.Status = EntityStatus.Declined;
                    finalState.SubStatus = EntitySubstatus.Synced;
                }
            }


            //merge SKUs
            if (!newState.Skus?.Any() ?? true)
                finalState.Skus = oldState?.Skus ?? new List<SkuIntegrationInfo>();

            finalState.Name = oldState?.Name ?? newState.Name;

            return newState;
        }
    }
}

