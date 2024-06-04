using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Services;
using Yah.Hub.Common.Services.CacheService;

namespace Yah.Hub.Application.Services.ThrottlingService
{
    public class ThrottlingService : AbstractService, IThrottlingService
    {
        private readonly ICacheService CacheService;

        public ThrottlingService(IConfiguration configuration, ILogger<ThrottlingService> logger, ICacheService cacheService) : base(configuration, logger)
        {
            this.CacheService = cacheService;
        }

        public async Task<bool> ConsumeThrottling(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage, string key, Func<int?>? getLimit)
        {
            var limit = await GetOrFetchRateLimit(marketplaceServiceMessage, key, getLimit);

            if (limit == null)
                return false;

            return await this.CacheService.SlideWindow(new Common.ServiceMessage.ServiceMessage<(string key, int limit, int windowSeconds)>(marketplaceServiceMessage.Identity, (key, (int)limit, 60)));
        }

        public async Task<long?> GetOrFetchRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage, string key, Func<int?>? getLimit)
        {
            // get and cache limit
            var cachedLimit = await this.CacheService.Get<int>(key.AsServiceMessage(marketplaceServiceMessage.Identity));
            int? limit = null;

            if (cachedLimit.Data == null || cachedLimit.Data == default(int))
            {
                int? mktLimit = getLimit();
                if(mktLimit != null)
                   await this.CacheService.Set<int>(new Common.ServiceMessage.ServiceMessage<(string key, int value, TimeSpan? expires, StackExchange.Redis.When? when)>(marketplaceServiceMessage.Identity, (key, (int)mktLimit, TimeSpan.FromHours(12), StackExchange.Redis.When.Always)));
                limit = mktLimit;
            }
            else
                limit = cachedLimit.Data;

            return limit;
        }
    }
}

