using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.BatchItem;

namespace Yah.Hub.Marketplace.Application.Catalog
{
    public interface IBatchCatalogService
    {
        public Task<ServiceMessage> ProcessBatchAsync(ServiceMessage<(BatchType BatchType, Operation CommandType)> serviceMessage);
    }
}
