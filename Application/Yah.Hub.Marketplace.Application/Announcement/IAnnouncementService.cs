using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using System;
namespace Yah.Hub.Marketplace.Application.Announcement
{
    public partial interface IAnnouncementService : ICatalogService
    {
        // sync marketplace methods
        public Task<ServiceMessage> ExecuteAnnouncementCommand(ServiceMessage<CommandMessage<Domain.Announcement.Announcement>> serviceMessage);
        public Task<ServiceMessage> ExecuteAnnouncementPriceCommand(ServiceMessage<CommandMessage<AnnouncementPrice>> serviceMessage);
        public Task<ServiceMessage> ExecuteAnnouncementInventoryCommand(ServiceMessage<CommandMessage<AnnouncementInventory>> serviceMessage);

        // sync service methods
        public Task<ServiceMessage<Domain.Announcement.Announcement>> CreateAnnouncement(MarketplaceServiceMessage<Domain.Announcement.Announcement> serviceMessage);
        public Task<ServiceMessage<Domain.Announcement.Announcement>> UpdateAnnouncement(MarketplaceServiceMessage<Domain.Announcement.Announcement> serviceMessage);
        public Task<ServiceMessage<Domain.Announcement.Announcement>> DeleteAnnouncement(MarketplaceServiceMessage<string> serviceMessage);
        public Task<ServiceMessage<Domain.Announcement.Announcement>> GetAnnouncementById(MarketplaceServiceMessage<string> serviceMessage);
        public Task<ServiceMessage> ChangeAnnouncementState(MarketplaceServiceMessage<(string announcementId, AnnouncementState state)> serviceMessage);

        // replication
        public Task<ServiceMessage> ReplicateAllAnnouncement(MarketplaceServiceMessage message);
        public Task<ServiceMessage> ReplicateAnnouncementById(MarketplaceServiceMessage<string> message);
        public Task<ServiceMessage> ReplicateAnnouncement(MarketplaceServiceMessage<Domain.Announcement.Announcement> message);

        // search
        public Task<ServiceMessage<AnnouncementSearchResult>> QueryAsync(MarketplaceServiceMessage<AnnouncementQuery> message);

        // consumers
        public Task<ServiceMessage> ConsumeAnnouncementCommand(ServiceMessage serviceMessage);
        public Task<ServiceMessage> ConsumeAnnouncementPriceCommand(ServiceMessage serviceMessage);
        public Task<ServiceMessage> ConsumeAnnouncementInventoryCommand(ServiceMessage serviceMessage);

        // producers
        public Task<ServiceMessage> EnqueueAnnouncementCommand(ServiceMessage<CommandMessage<Domain.Announcement.Announcement>> serviceMessage);
        public Task<ServiceMessage> EnqueueAnnouncementPriceCommand(ServiceMessage<CommandMessage<Domain.Announcement.AnnouncementPrice>> serviceMessage);
        public Task<ServiceMessage> EnqueueAnnouncementInventoryCommand(ServiceMessage<CommandMessage<Domain.Announcement.AnnouncementInventory>> serviceMessage);
        
    }
}

