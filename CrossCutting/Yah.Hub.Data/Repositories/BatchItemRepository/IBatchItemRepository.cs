using Yah.Hub.Common.AbstractRepositories.DynamoDB;
using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Data.Repositories.BatchItemRepository
{
    public interface IBatchItemRepository : IDynamoRepository<BatchItem>
    {
        public Task<ServiceMessage<List<BatchItem>>> QueryAsync(ServiceMessage<BatchItemQuery> serviceMessage);
    }
}
