using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.EntityInfos;

namespace Yah.Hub.Domain.Announcement
{
    public class AnnouncementWrapper
    {
        public AnnouncementWrapper(Announcement announcement, ProductIntegrationInfo integrationInfo)
        {
            Announcement = announcement;
            IntegrationInfo = integrationInfo;
        }

        public AnnouncementWrapper(Announcement announcement, PriceIntegrationInfo priceIntegrationInfo)
        {
            Announcement = announcement;
            PriceIntegrationInfo = priceIntegrationInfo;
        }

        public AnnouncementWrapper(Announcement announcement, InventoryIntegrationInfo inventoryIntegrationInfo)
        {
            Announcement = announcement;
            InventoryIntegrationInfo = inventoryIntegrationInfo;
        }

        public Announcement Announcement { get; set; }
        public ProductIntegrationInfo IntegrationInfo { get; set; }
        public PriceIntegrationInfo PriceIntegrationInfo { get; set; }
        public InventoryIntegrationInfo InventoryIntegrationInfo { get; set; }
    }
}
