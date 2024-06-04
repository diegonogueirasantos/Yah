using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;

namespace Yah.Hub.Data.Repositories.AnnouncementRepository
{
    public interface IAnnouncementSearchService
    {
        public Task<ServiceMessage<Announcement>> SaveAnnouncement(MarketplaceServiceMessage<Announcement> serviceMessage);
        public Task<ServiceMessage> SaveBulkAnnouncement(MarketplaceServiceMessage<List<Announcement>> serviceMessage);
        public Task<ServiceMessage<Announcement>> GetAnnouncementById(MarketplaceServiceMessage<string> serviceMessage);
        public Task<ServiceMessage<AnnouncementSearchResult>> QueryAsync(MarketplaceServiceMessage<AnnouncementQuery> message);

    }
}
