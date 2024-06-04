using System;
using System.Threading.Tasks;
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
        public async Task<ServiceMessage> HandleEntityState(CommandMessage<PriceIntegrationInfo> serviceMessage)
        {
            var result = new ServiceMessage(serviceMessage.Identity);

            try
            {
                var newState = new MarketplaceEntityState(serviceMessage.Data.Id, serviceMessage.Data.ReferenceId, serviceMessage.EventDateTime, serviceMessage.Identity.GetVendorId(), serviceMessage.Identity.GetTenantId(), serviceMessage.Identity.GetAccountId(), serviceMessage.Marketplace)
                {
                    ProductInfo = new ProductIntegrationInfo(serviceMessage.Data.Id, serviceMessage.Data.ReferenceId, Common.Enums.EntityStatus.Unknown, serviceMessage.EventDateTime)
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
                        PriceIntegrationInfo = mergedState
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
                        var oldPrice = oldState.ProductInfo.Skus.FirstOrDefault(x => x.Sku == serviceMessage.Data.Id)?.PriceIntegrationInfo;

                        //if (serviceMessage.Data.ServiceOperation == Common.Enums.Operation.Delete && !serviceMessage.Data.Data.Errors.Any())
                        //    await this.IntegrationMonitorRepository.DeleteAsync(oldState.Id.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));

                        var finalState = MergeEntityStates(serviceMessage, oldPrice);

                        // set new state
                        oldState.ProductInfo.Skus.FirstOrDefault(x => x.Sku == serviceMessage.Data.Id).PriceIntegrationInfo = finalState;

                        if (finalState != null)
                            await this.IntegrationMonitorRepository.SaveAsync(new Yah.Hub.Common.Marketplace.MarketplaceServiceMessage<MarketplaceEntityState>(serviceMessage.Identity, serviceMessage.Marketplace, oldState));

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

        public PriceIntegrationInfo MergeEntityStates(CommandMessage<PriceIntegrationInfo> source, PriceIntegrationInfo? oldState)
        {
            var newState = new PriceIntegrationInfo() { Retail = source.Data.Retail, DateTime = source.EventDateTime, SalePrice = source.Data.SalePrice, SalePriceFrom = source.Data.SalePriceFrom, SalePriceTo = source.Data.SalePriceTo };

            if (oldState == null && source.ServiceOperation != Common.Enums.Operation.Delete)
            {
                newState.SubStatus = Common.Enums.EntitySubstatus.Creating;
                newState.Status = Common.Enums.EntityStatus.Waiting;
            }
            return newState;
        }
    }
}

