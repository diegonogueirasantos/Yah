using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;

namespace Yah.Hub.Common.Clients.AnnouncementReplication.Interface
{
    public interface IAnnouncementReplicationClient
    {
        public Task<HttpMarketplaceMessage> ReplicateAnnouncementAsync(MarketplaceServiceMessage<Announcement> announcement);
    }
}
