using System;
using Yah.Hub.Api.Application.Category;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Category;

namespace Yah.Hub.MercadoLivre.Api.Controllers
{
	public class CategoryController : MarketplaceCategoryApi
    {

		public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService, ISecurityService securityService): base(categoryService, logger, securityService)
		{
		}

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }
    }
}

