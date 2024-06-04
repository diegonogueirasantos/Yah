using Yah.Hub.Api.Application.Order;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Sales;

namespace Yah.Hub.LeroyMerlin.Api.Controllers
{
    public class OrderController : MarketplaceOrderApi
    {
        public OrderController(ISalesService salesService, ILogger<OrderController> logger, ISecurityService securityService) : base(salesService, logger, securityService)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.LeroyMerlin;
        }
    }
}
