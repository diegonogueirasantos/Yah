using Yah.Hub.Domain.BatchItem;

namespace Yah.Hub.Domain.Monitor
{
    public class RequestInventoryBatchState
    {
        public RequestInventoryBatchState(string batchId)
        {
            BatchId = batchId;
        }

        public string BatchId { get; set; }
    }
}
