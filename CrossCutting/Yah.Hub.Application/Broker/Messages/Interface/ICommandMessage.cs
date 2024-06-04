namespace Yah.Hub.Marketplace.Application.Broker.Messages.Interface
{
    public interface ICommandMessage
    {
        Dictionary<string, object> Metadata { get; set; }
        public DateTimeOffset EventDateTime { get; set; }
    }

    public interface ICommandMessage<T> : ICommandMessage
    {
        T Data { get; set; }
    }
}
