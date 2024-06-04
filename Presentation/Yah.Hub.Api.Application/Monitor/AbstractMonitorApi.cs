using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Api.Application.Monitor
{
    public abstract class MarketplaceMonitorApi: MarketplaceControllerBase.MarketplaceControllerBase
    {
        IIntegrationMonitorService IntegrationMonitorService;
        ILogger Logger;

        protected MarketplaceMonitorApi(IIntegrationMonitorService integrationMonitorService, ILogger logger)
        {
            IntegrationMonitorService = integrationMonitorService;
            Logger = logger;
        }
    }
}
