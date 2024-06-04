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
using Yah.Hub.Domain.Monitor.EntityInfos;
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
        public async Task<ServiceMessage> HandleEntityState(CommandMessage<ProductInventory> serviceMessage)
        {
            var result = new ServiceMessage(serviceMessage.Identity);

            try
            {
                var newState = new MarketplaceEntityState(serviceMessage.Data.Id, serviceMessage.Data.Id, serviceMessage.EventDateTime, serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Marketplace)
                {
                    ProductInfo = new ProductIntegrationInfo(serviceMessage.Data.Id, serviceMessage.Data.IntegrationId, Common.Enums.EntityStatus.Unknown, serviceMessage.EventDateTime)
                };
                var currentState = await this.GetBtRefferId(newState.ReferenceId.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.Marketplace));

                if (!currentState.Data.Any())
                {
                    var mergedState = MergeEntityStates(serviceMessage, null);

                    newState.ProductInfo.Skus = new List<SkuIntegrationInfo>();
                    newState.ProductInfo.Skus.Add(new SkuIntegrationInfo()
                    {
                        Sku = serviceMessage.Data.Id,
                        Status = Common.Enums.EntityStatus.Waiting,
                        SubStatus = Common.Enums.EntitySubstatus.Creating,
                        InventoryIntegrationInfo = mergedState
                    });

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

                        // get priceState
                        var oldPrice = oldState.ProductInfo.Skus.FirstOrDefault(x => x.Sku == serviceMessage.Data.AffectedSku.Id)?.InventoryIntegrationInfo;

                        var finalState = MergeEntityStates(serviceMessage, oldPrice);

                        // set new state
                        oldState.ProductInfo.Skus.FirstOrDefault(x => x.Sku == serviceMessage.Data.AffectedSku.Id).InventoryIntegrationInfo = finalState;

                        if (finalState != null)
                            await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, oldState));

                    }
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        public InventoryIntegrationInfo MergeEntityStates(CommandMessage<ProductInventory> source, InventoryIntegrationInfo? oldState)
        {
            var newState = new InventoryIntegrationInfo() { Balance = source.Data.AffectedSku.Inventory.Balance, DateTime = source.EventDateTime, HandlingDays = source.Data.AffectedSku.Inventory.HandlingDays };

            if (oldState == null && source.ServiceOperation != Common.Enums.Operation.Delete)
            {
                newState.SubStatus = Common.Enums.EntitySubstatus.Creating;
                newState.Status = Common.Enums.EntityStatus.Waiting;
            }
            return newState;
        }

    }
}

