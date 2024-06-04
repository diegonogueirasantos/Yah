using Yah.Hub.Marketplace.Application.Broker.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Messages.Interface
{
    public interface ICommandInfo : IBrokerMessage
    {
        string CorrelationId { get; set; }

        string EventType { get; set; }

        string EventDateTime { get; set; }

        string CommandName { get; set; }

        string CommandDataType { get; set; }

        string TenantId { get; set; }

        string VendorId { get; set; }

        string AccountId { get; set; }

        string Username { get; set; }

        int? ReceiveCount { get; set; }
    }
}
