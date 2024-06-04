using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Query;

namespace Yah.Hub.Domain.BatchItem
{
    public class BatchItemQuery : Pagination
    {
        public BatchItemQuery(BatchStatus status, MarketplaceAlias marketplace, BatchType batchType, Operation commandType, int limit): base(limit)
        {
            Status = status;
            Marketplace = marketplace;
            BatchType = batchType;
            CommandType = commandType;
        }

        public BatchStatus Status { get; set; }
        public MarketplaceAlias Marketplace { get; set; }
        public BatchType BatchType { get; set; }
        public Operation CommandType { get; set; }
    }
}
