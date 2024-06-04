using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Domain.BatchItem
{
    public class BatchItem
    {
        public string EntityId { get; set; }
        public BatchStatus Status { get; set; }
        public string Data { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public BatchType Type { get; set; }
        public Operation CommandType { get; set; }
        public DateTimeOffset TTL { get; set; }
        public MarketplaceAlias Marketplace { get; set; }
        public Product Product { get; set; }
        public ProductPrice Price { get; set; }
        public ProductInventory Inventory { get; set; }
    }

    public enum BatchStatus
    {
        WAITING,
        PROCESSED
    }

    public enum BatchType
    {
        PRODUCT,
        INVENTORY,
        PRICE,
        IMAGE
    }
}
