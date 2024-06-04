using System;
using Nest;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Common.AbstractRepositories.ElasticSearch
{
    public interface IElasticSearchBaseRepository<T> where T : IBaseEntity
    {
        Task<ServiceMessage<T>> SaveAsync(MarketplaceServiceMessage<T> message);
        Task<ServiceMessage<T>> GetAsync(MarketplaceServiceMessage<string> message);
        Task<ServiceMessage<T>> DeleteAsync(MarketplaceServiceMessage<string> message);
        Task<ServiceMessage.ServiceMessage> SaveBulkAsync(MarketplaceServiceMessage<List<T>> message);
    }
}

