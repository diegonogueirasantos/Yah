using Yah.Hub.Api.Application.Category;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Category;

namespace Yah.Hub.ViaVarejo.Api.Controllers
{
    public class CategoryController : MarketplaceCategoryApi
    {
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger, ISecurityService securityService) : base(categoryService, logger, securityService)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.ViaVarejo;
        }
    }
}
