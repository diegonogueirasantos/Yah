using System;
using Hangfire;
using System.Linq.Expressions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;

namespace Yah.Hub.Scheduler.Services
{
    public class HangfireService : IHangfireService
    {
        private readonly ILogger<HangfireService> _logger;

        public HangfireService(ILogger<HangfireService> logger)
        {
            this._logger = logger;
        }

        public async Task<bool> AddOrUpdateTask(MarketplaceServiceMessage<(Expression<Action> executedMethod, string taskName,  string periodicity)> serviceMessage)
        {
            try
            {
                RecurringJob.AddOrUpdate(Guid.NewGuid().ToString(), serviceMessage.Data.executedMethod, Cron.MinuteInterval(Convert.ToInt32(serviceMessage.Data.periodicity)));
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveTask(Common.ServiceMessage.ServiceMessage<string> serviceMessage)
        {
            try
            {
                RecurringJob.RemoveIfExists($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}-{serviceMessage.Data}");
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return false;
            }
        }

        public string GetTaskKeys(MarketplaceServiceMessage<(Expression<Action> executedMethod, string taskName, string periodicity)> serviceMessage)
        {
            return $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}-{serviceMessage.AccountConfiguration.Marketplace}-{serviceMessage.Data.taskName}";
        }


        public string GetTaskKeys(string vendorId, string tenantId, string accountId, MarketplaceAlias marketplaceAlias, string taskName)
        {
            return $"{vendorId}-{tenantId}-{accountId}-{marketplaceAlias}-{taskName}";
        }


    }
}

