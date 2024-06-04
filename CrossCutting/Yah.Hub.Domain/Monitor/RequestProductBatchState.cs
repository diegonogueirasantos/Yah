using Yah.Hub.Domain.BatchItem;

namespace Yah.Hub.Domain.Monitor
{
    public class RequestProductBatchState
    {
        public RequestProductBatchState(string batchId)
        {
            BatchId = batchId;
        }

        public string BatchId { get ; set; }
    }
}
