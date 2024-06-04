using Yah.Hub.Api.Application.Catalog;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Catalog;

namespace Yah.Hub.Magalu.Api.Controllers
{
    public class CatalogController : MarketplaceCatalogApi
    {
        public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger, ISecurityService securityService) : base(catalogService, logger, securityService)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Magalu;
        }
    }
}
