using Yah.Hub.Application.Broker.Messages;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Broker.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Messages
{
    public class DequeueCommandBatchMessage<T> : BrokerBaseEntity<T>
    {
        public DequeueCommandBatchMessage()
        {
        }
        public List<DequeueCommandMessage> Commands { get; set; }
    }
}
