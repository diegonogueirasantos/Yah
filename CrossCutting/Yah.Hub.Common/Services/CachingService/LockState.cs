using StackExchange.Redis;
using Yah.Hub.Common.ServiceMessage;
using Nest;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Yah.Hub.Common.Services.CacheService;

namespace Yah.Hub.Common.Services.CachingService
{
    public class LockState : IDisposable
    {
        #region Properties
        private readonly TimeSpan ExpireTimeObsoleteKey = TimeSpan.FromDays(15);

        private readonly TimeSpan LockTime = TimeSpan.FromSeconds(30);

        private Identity.Identity Identity;

        private readonly ICacheService Service;
        public string Key { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool IsLocked { get; set; }
        public bool IsObsolete { get; set; }
        public TimeSpan LockTimeout { get; set; }

        #endregion

        #region Contructor

        public LockState(ICacheService service, ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)> message)
        {
            this.Service = service;
            this.Key = message.Data.key;
            this.Timestamp = message.Data.timestamp;
            this.LockTimeout = message.Data.lockTimeout;

            LockAsync(new ServiceMessage<(string key, DateTimeOffset timestamp)>(message.Identity, (message.Data.key, message.Data.timestamp))).GetAwaiter().GetResult(); // ???
        }
        #endregion

        #region Public Methods

        public async Task<LockState> LockAsync(ServiceMessage<(string key, DateTimeOffset timestamp)> serviceMessage)
        {
            Identity = serviceMessage.Identity;

            var lockKey = this.FormatTimestampKey(serviceMessage.Data.key);

            this.IsLocked = this.Service.Set<DateTimeOffset>(new ServiceMessage<(string key, DateTimeOffset value, TimeSpan? expires, When? when)>(serviceMessage.Identity, (lockKey, serviceMessage.Data.timestamp, LockTime, When.NotExists)))
                            .GetAwaiter()
                            .GetResult().Data;

            if (this.IsLocked)
            {
                var obsoleteMessage = new ServiceMessage<string>(serviceMessage.Identity, this.FormatTimestampKeyObsolete(serviceMessage.Data.key)) ;
                var currentLockTimestamp = this.Service.Get<DateTimeOffset>(obsoleteMessage).GetAwaiter().GetResult().Data;

                if (currentLockTimestamp != null)
                {
                    if(serviceMessage.Data.timestamp < currentLockTimestamp)
                    {
                        this.Service.Remove(new ServiceMessage<string>(Identity, this.FormatTimestampKey(this.Key)));

                        this.IsObsolete = true;
                        this.IsLocked = false;
                    }
                }    
            }

            return this;
        }

        public async Task<bool> PersistTimestampKeyAsync(ServiceMessage<LockState> message)
        {
            return await this.SetTimestampKeyAsync(message);
        }

        public void Dispose()
        {
            if (!this.IsLocked)
            {
                return;
            }

            this.Service.Remove(new ServiceMessage<string>(Identity, this.FormatTimestampKey(this.Key)));
        }

        #endregion

        #region Internal and Private Methods

        internal async Task<bool> SetTimestampKeyAsync(ServiceMessage<LockState> state)
        {
            if(!state.Data.IsLocked)
                return false;

            var keyObsolete = this.FormatTimestampKeyObsolete(state.Data.Key);

            return this.Service.Set<DateTimeOffset>(new ServiceMessage<(string key, DateTimeOffset value, TimeSpan? expires, When? when)>(state.Identity, (keyObsolete, state.Data.Timestamp, ExpireTimeObsoleteKey, When.NotExists)))
                    .GetAwaiter().GetResult().Data ;
        }

        private string FormatTimestampKeyObsolete(string key)
        {
            return $"o:{key}".ToLowerInvariant();
        }

        private string FormatTimestampKey(string key)
        {
            return $"{key}".ToLowerInvariant();
        }

        #endregion
    }


}
