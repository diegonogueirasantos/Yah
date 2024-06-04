using System;
using Amazon.DynamoDBv2;
using Nest;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Domain.Announcement
{
    
	public class Announcement : BaseEntity
    {
        public Announcement(string vendorId, string tenantId, string accountId, string id) 
        {
            this.VendorId = vendorId;
            this.TenantId = tenantId;
            this.AccountId = accountId;
            this.Id = id;
        }

        public string TenantId { get; private set; }
        public string AccountId { get; private set; }
        public string VendorId { get; private set; }
        public string? MarketplaceId { get; set; }
        public string ProductId { get; set; }
        public AnnouncementItem Item { get; set; }
        public Product Product { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPausedByMeli { get; set; }
        public DateTime Timestamp { get; set; }
        public MarketplaceAlias Marketplace { get; set; }
    }
}

