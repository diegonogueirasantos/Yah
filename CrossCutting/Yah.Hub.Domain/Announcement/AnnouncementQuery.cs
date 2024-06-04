using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Query;

namespace Yah.Hub.Domain.Announcement
{
    public class AnnouncementQuery : BaseQuery
    {
        public AnnouncementQuery() { }

        public string Title { get; set; }
        public string MarketplaceId { get; set; }
        public string VendorId { get; set; }
        public string TenantId { get; set; }
        public string AccountId { get; set; }
        public string AnnouncementId { get; set; }
        public string ProductId { get; set; }
        public string Category { get; set; }
        public EntityStatus? Status { get; set; }
    }
}
