using System;
using Yah.Hub.Api.Application.Catalog;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface;

namespace Yah.Hub.MercadoLivre.Api.Controllers
{
    public class AnnouncementController : MarketplaceAnnouncementApi
    {
        public AnnouncementController(IMercadoLivreCatalogService announcementService, ILogger<AnnouncementController> logger, ISecurityService securityService) : base(announcementService, logger, securityService)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }
    }
}

