using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Common.Extensions
{
    public static class ServiceMessageExtensions
    {
        public static ServiceMessage<T> AsServiceMessage<T>(this T model, Identity.Identity identity)
        {
            return new ServiceMessage<T>(identity, model);
        }

        public static ServiceMessage.ServiceMessage AsServiceMessage(this Identity.Identity identity)
        {
            return new ServiceMessage.ServiceMessage(identity);
        }

        public static T Merge<T>(this T result, ServiceMessage.ServiceMessage resultToMerge)
            where T : ServiceMessage.ServiceMessage
        {
            if (resultToMerge == null)
                return result;

            foreach (var e in resultToMerge.Errors)
                result.WithError(e);

            return result;
        }
    }

}
