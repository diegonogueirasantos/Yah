using System;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Monitor;
﻿using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using System;
using Yah.Hub.Common.Enums;
using Nest;
using Yah.Hub.Domain.Monitor.Query;

namespace Yah.Hub.Application.Services.IntegrationMonitorService
{
	public partial interface IIntegrationMonitorService
	{
        public Task<ServiceMessage> MonitorIntegrationStatus(MarketplaceServiceMessage<(EntityType type, bool HasBatchSystem)> serviceMessage);
        public Task<ServiceMessage<List<IntegrationSummary>>> GetIntegrationSummary(MarketplaceServiceMessage serviceMessage);
        public Task<ServiceMessage<EntityStateSearchResult>> QueryAsync(MarketplaceServiceMessage<EntityStateQuery> message);
        public Task<ISearchResponse<MarketplaceEntityState>> GetEntityStateByBatchId(MarketplaceServiceMessage<BatchQueryRequest> message);
        public Task<ServiceMessage<List<bool>>> HandleCommands();
    }
}

