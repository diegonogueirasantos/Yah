using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.BatchItemRepository;

namespace Yah.Hub.Application.Services.BatchItemService
{
    public class BatchItemService : AbstractService, IBatchItemService
    {
        private IBatchItemRepository BatchItemRepository {get;}

        public BatchItemService(IConfiguration configuration, ILogger<BatchItemService> logger, IBatchItemRepository batchItemRepository) : base(configuration, logger)
        {
            BatchItemRepository = batchItemRepository;
        }

        public async Task<ServiceMessage<List<BatchItem>>> GetBachItems(ServiceMessage<BatchItemQuery> batchItemQuery)
        {
            return await this.BatchItemRepository.QueryAsync(batchItemQuery);
        }

        public async Task<ServiceMessage> SaveBatchItem(ServiceMessage<BatchItem> serviceMessage)
        {
            if (serviceMessage.Data.Status.Equals(BatchStatus.PROCESSED))
            {
                serviceMessage.Data.TTL = DateTimeOffset.Now.AddDays(Convert.ToInt32(base.Configuration["BatchConfig:TTL_PROCESSED_DAYS"]));
            }
            else
            {
                serviceMessage.Data.TTL = DateTimeOffset.Now.AddDays(Convert.ToInt32(base.Configuration["BatchConfig:TTL_WAITING_DAYS"]));
            }

            return await this.BatchItemRepository.UpsertItem(serviceMessage);
        }
    }
}
