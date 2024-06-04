using System;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Application.Services.ThrottlingService
{
    public interface IThrottlingService
    {
        public Task<bool> ConsumeThrottling(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage, string key, Func<int?>? getLimit);
    }
}

