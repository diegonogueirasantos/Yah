using System;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Application.Services.IntegrationMonitorService
{
    public partial interface IIntegrationMonitorService
    {
        Task<ServiceMessage> HandleEntityState(CommandMessage<Announcement> serviceMessage);
    }
}

