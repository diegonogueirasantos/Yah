using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.AnnouncementRepository;
using Yah.Hub.Domain.Announcement;

namespace Yah.Hub.Application.Services.AnnouncementService
{
    public class AnnouncementSearchService : AbstractService, IAnnouncementSearchService
    {
        private readonly IAnnouncementSearchRepository AnnouncementSearchRepository;

        public AnnouncementSearchService(IConfiguration configuration, ILogger<AnnouncementSearchService> logger, IAnnouncementSearchRepository announcementSearchRepository) : base(configuration, logger)
        {
            this.AnnouncementSearchRepository = announcementSearchRepository;
        }

        public async Task<ServiceMessage<Announcement>> GetAnnouncementById(MarketplaceServiceMessage<string> serviceMessage)
        {
            return await this.AnnouncementSearchRepository.GetAsync(serviceMessage);
        }

        public async Task<ServiceMessage<Announcement>> SaveAnnouncement(MarketplaceServiceMessage<Announcement> serviceMessage)
        {
            return await this.AnnouncementSearchRepository.SaveAsync(serviceMessage);
        }

        public async Task<ServiceMessage> SaveBulkAnnouncement(MarketplaceServiceMessage<List<Announcement>> serviceMessage)
        {
            return await this.AnnouncementSearchRepository.SaveBulkAsync(serviceMessage);
        }

        public async Task<ServiceMessage<AnnouncementSearchResult>> QueryAsync(MarketplaceServiceMessage<AnnouncementQuery> message)
        {
            message = message ?? throw new ArgumentNullException(nameof(message));

            return await this.AnnouncementSearchRepository.QueryAsync(message);
        }
    }
}
