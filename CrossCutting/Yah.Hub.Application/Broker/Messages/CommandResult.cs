namespace Yah.Hub.Marketplace.Application.Broker.Messages
{
    public class CommandResult<T>
    {
        public string MessageId { get; set; }

        public string ReceiptHandle { get; set; }

        public int ReceiveCount { get; set; }

        public T Command { get; set; }

        public string CommandDataType { get; set; }
        
        public Dictionary<string, string> Attributes { get; set; }
    }
}
