using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services.CachingService;
using StackExchange.Redis;

namespace Yah.Hub.Common.Services.CacheService
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer Connection;
        private readonly IDatabase Caching;
        private readonly IConfiguration Configuration;
        // Max 30 days
        private TimeSpan MaxExpires = new TimeSpan(720, 0, 0);
        private string FormatKey(ServiceMessage<string> serviceMessage) 
        {
            var identityKey = string.Empty;
            var env = Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value.ToLower();

            if (serviceMessage.Identity.IsValidVendorTenantAccountIdentity())
            {
                identityKey = $"vta:{serviceMessage.Identity.GetVendorId()}:{serviceMessage.Identity.GetTenantId()}:{serviceMessage.Identity.GetAccountId()}:{serviceMessage.Data}";
            }
            else if (serviceMessage.Identity.IsValidVendorTenantIdentity())
            {
                identityKey = $"vt:{serviceMessage.Identity.GetVendorId()}:{serviceMessage.Identity.GetTenantId()}:{serviceMessage.Data}";
            }
            else if (serviceMessage.Identity.IsValidVendorIdentity())
            {
                identityKey = $"v:{serviceMessage.Identity.GetVendorId()}:{serviceMessage.Data}";
            }else if (serviceMessage.Identity.IsValidWorkerIdentity())
            {
                identityKey = "w";
            }

            return $"{env}:{identityKey}";
        } 

        public CacheService(IConnectionMultiplexer connection, IConfiguration configuration)
        {
            Connection = connection;
            this.Caching = connection.GetDatabase();
            Configuration = configuration;
        }

        public async Task<ServiceMessage<T>> Get<T>(ServiceMessage<string> key)
        {
            var result = new ServiceMessage<T>(key.Identity);
            try
            {
                var value = await Caching.StringGetAsync(new RedisKey(FormatKey(key)));
                if (value.HasValue)
                    result.Data = JsonConvert.DeserializeObject<T>(value.ToString());
            }
            catch (Exception ex)
            {
                //Logger.LogCustomCritical($"ERROR on GET key: {key.Data}, ERROR: {Newtonsoft.Json.JsonConvert.SerializeObject(ex)}"); // just for cluster debug
                result.WithError(new Error(ex));
            }

            return result;
        }

        public async Task<ServiceMessage<bool>> Set<T>(ServiceMessage<(string key, T? value, TimeSpan? expires, When? when)> serviceMessage)
        {
            var result = new ServiceMessage<bool>(serviceMessage.Identity);

            try
            {
                result.Data = await Caching.StringSetAsync(
                FormatKey(new ServiceMessage<string>(serviceMessage.Identity, serviceMessage.Data.key)),
                JsonConvert.SerializeObject(serviceMessage.Data.value),
                serviceMessage.Data.expires ?? MaxExpires,
                serviceMessage.Data.when ?? When.Always,
                CommandFlags.None);
            }
            catch (Exception ex)
            {
                //Logger.LogCustomCritical($"ERROR on SET key: {serviceMessage.Data.key}, ERROR: {Newtonsoft.Json.JsonConvert.SerializeObject(ex)}");
                result.WithError(new Error(ex));
            }

            return result;
        }

        public async Task<ServiceMessage<int>> IncrementKey(ServiceMessage<(string key, int value, TimeSpan? expires)> serviceMessage)
        {
            var result = new ServiceMessage<int>(serviceMessage.Identity);

            try
            {
                var key = FormatKey(new ServiceMessage<string>(serviceMessage.Identity, serviceMessage.Data.key));
                var expire = serviceMessage.Data.expires.HasValue ? (double?)serviceMessage.Data.expires.Value.TotalSeconds : (double)MaxExpires.TotalSeconds;

                // create, increase and set expire to a key
                var incr = @$"local count = redis.call(""incr"", ""{key}"");
                              redis.call(""expire"", ""{key}"", ""{expire}"");
                              return count";

                var res = Caching.ScriptEvaluate(incr);

                if (((RedisValue)res).HasValue)
                    result.Data = ((int)((RedisValue)res));
            }
            catch (Exception ex)
            {
                //Logger.LogCustomCritical($"ERROR on INCREASE key: {serviceMessage.Data.key}, ERROR: {Newtonsoft.Json.JsonConvert.SerializeObject(ex)}");
                result.WithError(new Error(ex));
            }

            return result;
        }

        public async Task<ServiceMessage<bool>> Remove(ServiceMessage<string> serviceMessage)
        {
            var result = new ServiceMessage<bool>(serviceMessage.Identity);

            try
            {
                result.Data = await Caching.KeyDeleteAsync(FormatKey(serviceMessage));
            }
            catch (Exception ex)
            {
                //Logger.LogCustomCritical($"ERROR on REMOVE key: {serviceMessage.Data}, ERROR: {Newtonsoft.Json.JsonConvert.SerializeObject(ex)}");
                result.WithError(new Error(ex));
            }

            return result;
        }

        /// <summary>
        /// Verify, increment and return slide window limit
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public async Task<bool> SlideWindow(ServiceMessage<(string key, int limit, int windowSeconds)> serviceMessage)
        {
            var key = this.FormatKey(new ServiceMessage<string>(serviceMessage.Identity, serviceMessage.Data.key));

            var limited = ((int)await Caching.ScriptEvaluateAsync(Scripts.SlidingRateLimiterScript,
            new { key = new RedisKey($"{key}:slideWindow"), window = serviceMessage.Data.windowSeconds, max_requests = serviceMessage.Data.limit })) == 1;
            return limited;
        }
    }
}