using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Application.Services.BatchItemService
{
    public interface IBatchItemService
    {
        public Task<ServiceMessage<List<BatchItem>>> GetBachItems(ServiceMessage<BatchItemQuery> batchItemQuery);
        public Task<ServiceMessage> SaveBatchItem(ServiceMessage<BatchItem> serviceMessage);
    }
}
