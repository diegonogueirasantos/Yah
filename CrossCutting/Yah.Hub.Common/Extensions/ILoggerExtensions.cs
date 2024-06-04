using System;
using Microsoft.Extensions.Logging;
using Yah.Hub.Common.Security;
using Microsoft.Extensions.Logging;
using Yah.Hub.Common.ServiceMessage;
using Elasticsearch.Net;
using Newtonsoft.Json;

namespace Yah.Hub.Common.Extensions
{
    public static class ILoggerExtensions
    {
        private static JsonSerializerSettings JsonSerializeSettings { get; set; } = new JsonSerializerSettings
        { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };


        public static void LogCustomCritical(this ILogger logger, Common.ServiceMessage.Error error, Identity.Identity identity, object? obj = null)
        {
            logger.LogWithContext(
                () => logger.LogCritical(error.Message),
                    new KeyValuePair<string, object>("VendorId", identity.GetVendorId()),
                    new KeyValuePair<string, object>("TenantId", identity.GetTenantId()),
                    new KeyValuePair<string, object>("AccountId", identity.GetAccountId()),
                    new KeyValuePair<string, object>("Reason", error.Reason),
                    new KeyValuePair<string, object>("ErrorType", error.Type.ToString()),
                    new KeyValuePair<string, object>("OriginalException", Newtonsoft.Json.JsonConvert.SerializeObject(error.OriginalExcetion.StackTrace, JsonSerializeSettings)),
                    new KeyValuePair<string, object>("Object", Newtonsoft.Json.JsonConvert.SerializeObject(obj, JsonSerializeSettings)));
        }

        public static void LogCustomCritical(this ILogger logger, string message, Identity.Identity identity, object? obj = null)
        {
            logger.LogWithContext(
                () => logger.LogCritical(message),
                    new KeyValuePair<string, object>("VendorId", identity.GetVendorId()),
                    new KeyValuePair<string, object>("TenantId", identity.GetTenantId()),
                    new KeyValuePair<string, object>("AccountId", identity.GetAccountId()),
                    new KeyValuePair<string, object>("Object", Newtonsoft.Json.JsonConvert.SerializeObject(obj, JsonSerializeSettings)));
        }

        public static void LogCustomError(this ILogger logger, Common.ServiceMessage.Error error, Identity.Identity identity, object? obj = null)
        {
            logger.LogWithContext(
                () => logger.LogError(error.Message),
                    new KeyValuePair<string, object>("VendorId", identity.GetVendorId()),
                    new KeyValuePair<string, object>("TenantId", identity.GetTenantId()),
                    new KeyValuePair<string, object>("AccountId", identity.GetAccountId()),
                    new KeyValuePair<string, object>("Reason", error.Reason),
                    new KeyValuePair<string, object>("ErrorType", error.Type.ToString()),
                    new KeyValuePair<string, object>("OriginalException", Newtonsoft.Json.JsonConvert.SerializeObject(error.OriginalExcetion.StackTrace, JsonSerializeSettings)),
                    new KeyValuePair<string, object>("Object", Newtonsoft.Json.JsonConvert.SerializeObject(obj, JsonSerializeSettings)));
        }

        public static void LogCustomError(this ILogger logger, string message, Identity.Identity identity, object? obj = null)
        {
            logger.LogWithContext(
                () => logger.LogError(message),
                    new KeyValuePair<string, object>("VendorId", identity.GetVendorId()),
                    new KeyValuePair<string, object>("TenantId", identity.GetTenantId()),
                    new KeyValuePair<string, object>("AccountId", identity.GetAccountId()),
                    new KeyValuePair<string, object>("Object", Newtonsoft.Json.JsonConvert.SerializeObject(obj, JsonSerializeSettings)));
        }

        public static void LogCustomInformation(this ILogger logger, string message, Identity.Identity identity, object? obj = null)
        {
            logger.LogWithContext(
                () => logger.LogInformation(message),
                    new KeyValuePair<string, object>("VendorId", identity.GetVendorId()),
                    new KeyValuePair<string, object>("TenantId", identity.GetTenantId()),
                    new KeyValuePair<string, object>("AccountId", identity.GetAccountId()),
                    new KeyValuePair<string, object>("Object", Newtonsoft.Json.JsonConvert.SerializeObject(obj, JsonSerializeSettings)));  
        }


        private static void LogWithContext(this ILogger logger, Action LogAction,
        params KeyValuePair<string, object>[] contextDataParam)
        {
            Dictionary<string, object> contextData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kvp in contextDataParam)
            {
                contextData.TryAdd(kvp.Key, kvp.Value);
            }

            using (logger.BeginScope(contextData))
            {
                LogAction.Invoke();
            };
        }

        private static KeyValuePair<string, object>[] GetIdentityInformation(Identity.Identity identity)
        {
            return new KeyValuePair<string, object>[]  {
                    new KeyValuePair<string, object>("VendorId", identity.GetVendorId()),
                    new KeyValuePair<string, object>("TenantId", identity.GetTenantId()),
                    new KeyValuePair<string, object>("AccountId", identity.GetAccountId())
                    };
        }
    }
}