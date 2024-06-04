using System;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Notification;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;

namespace Yah.Hub.Marketplace.Application.Notification
{
	public abstract class NotificationService<T> : AbstractMarketplaceService, INotificationService<T>
    {
        private IBrokerService BrokerService { get; set; }
        protected IAccountConfigurationService ConfigurationService { get; }

        public NotificationService(IConfiguration configuration,
            ILogger<NotificationService<T>> logger, IBrokerService brokerService, IAccountConfigurationService accountConfigurationService) : base(configuration, logger)
		{
            this.BrokerService = brokerService;
            this.ConfigurationService = accountConfigurationService;
        }

        #region Producers

        public virtual async Task<ServiceMessage> EnqueueNotificationCommand(ServiceMessage<NotificationEvent<T>> serviceMessage)
        {


            var command = new CommandMessage<NotificationEvent<T>>(serviceMessage.Identity)
            {
                CorrelationId = Guid.NewGuid().ToString(),
                Data = serviceMessage.Data
            };
            
            
            return await this.BrokerService.EnqueueCommandAsync<NotificationEvent<T>>(command);
        }

        public virtual async Task<ServiceMessage> ConsumeNotificationCommands(ServiceMessage serviceMessage)
        {
            var result = ServiceMessage.CreateValidResult(serviceMessage.Identity);

            try
            {
                var messageBatches = this.BrokerService.PeekCommand<CommandMessage<NotificationEvent<T>>>(new CommandMessage<NotificationEvent<T>>(serviceMessage.Identity));

                foreach (var batch in messageBatches)
                {
                    var dequeueBatch = new DequeueCommandBatchMessage<NotificationEvent<T>>();
                    dequeueBatch.Commands = new List<DequeueCommandMessage>();

                    // TODO: CREATE A TASK TO EACH MESSAGE AND USE WHEN ALL PER BATCH TO DEQUEUE
                    Parallel.ForEach(batch, async (message) =>
                    {
                        // consume
                        var consumeResult = this.ProcessNotification(message.Command.Data.AsMarketplaceServiceMessage(message.Command.Identity, this.GetMarketplace())).GetAwaiter().GetResult();

                        // if succes add to dequeue array
                        if (consumeResult.IsValid)
                            dequeueBatch.Commands.Add(new DequeueCommandMessage() { Marketplace = GetMarketplace().ToString(), MessageId = message.MessageId, ReceiptHandle = message.ReceiptHandle });
                    });

                    if (dequeueBatch.Commands.Any())
                    {
                        var dequeueResult = this.BrokerService.DequeueCommandBatchAsync<NotificationEvent<T>>(dequeueBatch.AsServiceMessage(serviceMessage.Identity)).GetAwaiter().GetResult();
                        result.WithErrors(dequeueResult.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to consume notifications");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public abstract Task<ServiceMessage> ProcessNotification(MarketplaceServiceMessage<NotificationEvent<T>> notificationEvent);
 

        #endregion
    }
}