using Yah.Hub.Api.Application.Monitor;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.B2W.Api.Controllers
{
    public class MonitorController : MarketplaceMonitorApi
    {
        public MonitorController(IIntegrationMonitorService integrationMonitorService, ILogger<MonitorController> logger) : base(integrationMonitorService, logger)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.B2W;
        }
    }
}
