using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;

namespace Yah.Hub.Data.Repositories.AnnouncementRepository
{
    public interface IAnnouncementSearchRepository : IElasticSearchBaseRepository<Domain.Announcement.Announcement>
    {
        public Task<ServiceMessage<AnnouncementSearchResult>> QueryAsync(MarketplaceServiceMessage<AnnouncementQuery> message);
    }
}
