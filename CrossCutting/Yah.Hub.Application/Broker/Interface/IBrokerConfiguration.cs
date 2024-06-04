using Yah.Hub.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Interface
{
    public interface IBrokerConfiguration
    {
        string ResolveTopic<T>(BrokerBaseEntity<T> message);

        string ResolveSubject<T>(BrokerBaseEntity<T> message);

        string ResolveQueueUrl<T>(T message);

        string ResolveDLQueueUrl<T>(BrokerBaseEntity<T> message);

        int ResolveBatchSize<T>(T message);

        int ResolveVisibility<T>(T message);


    }
}
