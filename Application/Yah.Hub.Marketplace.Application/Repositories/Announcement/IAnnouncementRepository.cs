using System;
using Yah.Hub.Common.AbstractRepositories.DynamoDB;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Marketplace.Application.Repositories.Announcement
{
    public interface IAnnouncementRepository : IDynamoRepository<Domain.Announcement.Announcement>
    {
        public Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetItemByProductId(ServiceMessage<string> serviceMessage);
        public Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetAllAnnouncementsByAccount(ServiceMessage serviceMessage);
        public Task<ServiceMessage<Domain.Announcement.Announcement>> GetItemById(ServiceMessage<string> serviceMessage);
        public Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetItemByMarketplaceId(ServiceMessage<string> serviceMessage);
    }
}
