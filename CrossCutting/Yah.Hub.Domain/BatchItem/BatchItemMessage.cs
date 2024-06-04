using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Domain.BatchItem
{
    public class BatchItemMessage
    {
        public MarketplaceAlias Marketplace { get; set; }
        public BatchType BatchType { get; set; }
        public BatchStatus Status { get; set; }
        public Operation CommandType { get; set; }
        public int MaxDoc { get; set; }
    }
}
