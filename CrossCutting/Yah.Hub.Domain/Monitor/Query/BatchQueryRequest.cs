using Yah.Hub.Domain.BatchItem;

namespace Yah.Hub.Domain.Monitor.Query
{
    public class BatchQueryRequest : ElasticBasicScroll
    {
        public BatchQueryRequest(int maxItemsPerExecution, TimeSpan scrollTime, BatchType batchType, string batchId) : base(maxItemsPerExecution, scrollTime)
        {
            BatchId = batchId;
            BatchType = batchType;
        }

        public BatchType BatchType { get; set; }
        public string BatchId { get; set; }
    }
}
