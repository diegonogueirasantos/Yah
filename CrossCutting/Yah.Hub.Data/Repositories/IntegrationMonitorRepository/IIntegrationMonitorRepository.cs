using System;
using Nest;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Monitor.Query;

namespace Yah.Hub.Data.Repositories.IntegrationMonitorRepository
{
	public interface  IIntegrationMonitorRepository : IElasticSearchBaseRepository<MarketplaceEntityState>
    {

        public Task<ServiceMessage<List<MarketplaceEntityState>>> GetByReferenceId(MarketplaceServiceMessage<string> message);
        public Task<ServiceMessage<List<IntegrationSummary>>> GetIntegrationSummary(MarketplaceServiceMessage serviceMessage);
        public Task<ISearchResponse<MarketplaceEntityState>> GetEntitiesByStatusRequest(MarketplaceServiceMessage<MonitorStatusRequest> message, EntityType type);
        public Task<ServiceMessage<EntityStateSearchResult>> QueryAsync(MarketplaceServiceMessage<EntityStateQuery> message);
        public Task<ISearchResponse<MarketplaceEntityState>> GetEntitiesByBatchId(MarketplaceServiceMessage<BatchQueryRequest> message);

    }
}

