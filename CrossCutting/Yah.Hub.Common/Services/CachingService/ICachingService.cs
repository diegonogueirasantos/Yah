using System;
using Yah.Hub.Common.ServiceMessage;
using StackExchange.Redis;

namespace Yah.Hub.Common.Services.CacheService
{
    public interface ICacheService
    {
        public Task<ServiceMessage<T>> Get<T>(ServiceMessage<string> key);
        public Task<ServiceMessage<bool>> Set<T>(ServiceMessage<(string key, T value, TimeSpan? expires, When? when)> serviceMessage);
        public Task<ServiceMessage<bool>> Remove(ServiceMessage<string> key);
        public Task<ServiceMessage<int>> IncrementKey(ServiceMessage<(string key, int value, TimeSpan? expires)> serviceMessage);
        public Task<bool> SlideWindow(ServiceMessage<(string key, int limit, int windowSeconds)> serviceMessage);
    }
}

