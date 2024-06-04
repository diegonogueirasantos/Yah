using System;
using Yah.Hub.Common.Notification;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Marketplace.Application.Notification
{
    public interface INotificationService<T>
    {
        Task<ServiceMessage> EnqueueNotificationCommand(ServiceMessage<NotificationEvent<T>> serviceMessage);
        Task<ServiceMessage> ConsumeNotificationCommands(ServiceMessage serviceMessage);
    }
}

