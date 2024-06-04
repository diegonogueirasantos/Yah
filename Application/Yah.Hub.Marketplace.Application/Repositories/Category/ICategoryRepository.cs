using System;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Category;

namespace Yah.Hub.Marketplace.Application.Repositories
{
    public interface ICategoryRepository : IElasticSearchBaseRepository<MarketplaceCategory>
    {
        public Task<ServiceMessage<List<MarketplaceCategory>>> GetRootCategories(MarketplaceServiceMessage serviceMessage);
    }
}

