using Yah.Hub.Api.Application.Monitor;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.LeroyMerlin.Api.Controllers
{
    public class MonitorController : MarketplaceMonitorApi
    {
        public MonitorController(IIntegrationMonitorService integrationMonitorService, ILogger logger) : base(integrationMonitorService, logger)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.LeroyMerlin;
        }
    }
}
