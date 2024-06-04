using Yah.Hub.Marketplace.Application.Broker.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Messages
{
    public class DequeueCommandMessage : IBrokerMessage
    {
        public string MessageId { get; set; }

        public string ReceiptHandle { get; set; }

        public string Marketplace { get; set; }
    }
}
