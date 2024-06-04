using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Announcement;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface
{
    public interface IMercadoLivreCatalogService : IAnnouncementService
    {
        // PS: Avoid specific implementations, when TAGS and SCORE feature got implemented, replace this for RequestState abstract method
        Task<ServiceMessage> ResyncAnnouncement(MarketplaceServiceMessage<string> marketplaceServiceMessage);
    }
}
