using Yah.Hub.Application.Broker.Messages;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;

namespace Yah.Hub.Marketplace.Application.Broker.Messages
{
    public class CommandMessage<T> : BrokerBaseEntity<T>
    {
        public CommandMessage(Identity identity)
        {
            Identity = identity;
            this.EventDateTime = DateTimeOffset.Now;
        }

        public string CorrelationId { get; set; }

        /// <summary>
        /// REQUESTED, EXECUTED
        /// </summary>
        public ExecutionStep ExecutionStep { get; set; }

        public bool IsSync { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

        public int? ReceiveCount { get; set; }
       
        public DateTimeOffset EventDateTime { get; set; }

        public Operation ServiceOperation { get; set; }

        public Identity Identity { get; set; }

        public MarketplaceAlias Marketplace { get; set; }

        public bool IsAnnouncement  { get; set; }
     }
}
