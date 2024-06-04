using Yah.Hub.Api.Application.Catalog;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Catalog;

namespace Yah.Hub.LeroyMerlin.Api.Controllers
{
    public class CatalogController : MarketplaceBatchCatalogApi
    {
        public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger, ISecurityService securityService, IBatchCatalogService batchCatalogService) : base(catalogService, logger, securityService, batchCatalogService)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.LeroyMerlin;
        }
    }
}
