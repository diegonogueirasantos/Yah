using System;
using System.Linq.Expressions;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Scheduler.Services
{
    public interface IHangfireService
    {
        public Task<bool> AddOrUpdateTask(MarketplaceServiceMessage<(Expression<Action> executedMethod, string taskName, string periodicity)> serviceMessage);
        Task<bool> RemoveTask(Common.ServiceMessage.ServiceMessage<string> serviceMessage);
        public string GetTaskKeys(MarketplaceServiceMessage<(Expression<Action> executedMethod, string taskName, string periodicity)> serviceMessage);


        public string GetTaskKeys(string vendorId, string tenantId, string accountId, MarketplaceAlias marketplaceAlias, string taskName);
       
    }
}

