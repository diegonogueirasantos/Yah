namespace Yah.Hub.Marketplace.Application.Broker.Messages
{
    public class DequeueCommandResult
    {
        public string MessageId { get; set; }

        public string ReceiptHandle { get; set; }

        public bool IsSuccess { get; set; }
    }
}
