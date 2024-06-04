using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Monitor.EntityInfos;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Services.IntegrationMonitorService
{
	public partial interface IIntegrationMonitorService
    {
        Task<ServiceMessage> HandleEntityState(CommandMessage<PriceIntegrationInfo> serviceMessage);
    }
}

