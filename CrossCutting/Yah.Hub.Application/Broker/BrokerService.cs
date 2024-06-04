using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService;
using Amazon.SQS.Model;
using Amazon.SQS;
using Newtonsoft.Json;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Common.Extensions;
using AwsSns = Amazon.SimpleNotificationService.Model;
using Yah.Hub.Marketplace.Application.Broker.Messages.Interface;
using Yah.Hub.Application.Broker.Messages;
using Yah.Hub.Common.Security;
using Nest;

namespace Yah.Hub.Marketplace.Application.Broker
{
    public class BrokerService : AbstractService, IBrokerService
    {
        private const int MaxNumberOfMessages = 10;

        private readonly IAmazonSimpleNotificationService amazonSimpleNotificationService;
        private readonly IAmazonSQS amazonSimpleQueueService;
        private readonly IBrokerConfiguration brokerConfiguration;

        #region Constructor

        public BrokerService(
            IConfiguration configuration,
            ILogger<BrokerService> logger,
            IBrokerConfiguration brokerConfiguration,
            IAmazonSimpleNotificationService amazonSimpleNotificationService,
            IAmazonSQS amazonSimpleQueueService)
            : base(configuration, logger)
        {
            this.brokerConfiguration = brokerConfiguration ?? throw new ArgumentNullException(nameof(brokerConfiguration));
            this.amazonSimpleNotificationService = amazonSimpleNotificationService ?? throw new ArgumentNullException(nameof(amazonSimpleNotificationService));
            this.amazonSimpleQueueService = amazonSimpleQueueService ?? throw new ArgumentNullException(nameof(amazonSimpleQueueService));
        }

        #endregion

        #region IBrokerService Members

        /// <summary>
        /// Enqueue command.
        /// </summary>
        public async Task<ServiceMessage> EnqueueCommandAsync<T>(CommandMessage<T> message)
        {
            var result = ServiceMessage.CreateValidResult(message.Identity);

            var topic = brokerConfiguration.ResolveTopic(message);
            var subject = brokerConfiguration.ResolveSubject(message);

            var messageAttributes = new Dictionary<string, AwsSns.MessageAttributeValue>
            {
                { "vendorId", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = Convert.ToString(message.Identity.GetVendorId()) } },
                { "tenantId", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = Convert.ToString(message.Identity.GetTenantId()) } },
                { "accountId", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = Convert.ToString(message.Identity.GetAccountId()) } },
                { "commandDataType", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = message.CommandDataType.RemoveAccents().OnlyLetters() } },
                { "eventType", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = message.ServiceOperation.ToString() } },
                { "marketplace", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = message.Marketplace.ToString().ToLower() } },
                { "environment", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = this.GetEnvironment() } },
                { "executionMode", new AwsSns.MessageAttributeValue() { DataType = "String", StringValue = message.IsSync ? "sync" : "async" } }
            };

            message.EventDateTime = DateTimeOffset.Now;

            var messageToPublish = JsonConvert.SerializeObject(message);
            var compressedMessage = messageToPublish.CompressToGzip();

            try
            {
                var response = await amazonSimpleNotificationService.PublishAsync(
                    new PublishRequest()
                    {
                        TopicArn = topic,
                        Subject = subject,
                        Message = compressedMessage,
                        MessageAttributes = messageAttributes
                    }
                );

                Logger.LogTrace(
                    $"Broker.EnqueueCommandAsync {response.HttpStatusCode} " + // Prefix
                    $"|{typeof(T).Name}|{message.Marketplace}|{message.Identity.GetVendorId()}-{message.Identity.GetTenantId()}|{typeof(T).Name}|" + // Keyword
                    $" {compressedMessage}"); // Message
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while enqueue message");
                Logger.LogCustomCritical(error, message.Identity, message);
                result.WithError(error);
            }

            return result;
        }

        /// <summary>
        /// Pega comandos da fila de processamento de acordo com o marketplace 
        /// </summary>
        public IEnumerable<CommandResult<T>[]> PeekCommand<T>(T message) 
        {
            var queueUrl = brokerConfiguration.ResolveQueueUrl(message);
            var batchSize = brokerConfiguration.ResolveBatchSize(message);
            int commandCount = 0;

            while (commandCount < batchSize)
            {
                var pageSize = (batchSize - commandCount) >= MaxNumberOfMessages
                    ? MaxNumberOfMessages
                    : batchSize - commandCount;

                ReceiveMessageResponse response = null;
                try
                {
                    response = amazonSimpleQueueService.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = queueUrl,
                        MaxNumberOfMessages = pageSize,
                        VisibilityTimeout = brokerConfiguration.ResolveVisibility(message),
                        AttributeNames = new List<string> { "ApproximateReceiveCount" }

                    }).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Error error = new Error(ex, "Error while peek message");
                    Logger.LogCritical(ex, error.Message);
                }

                // Empty Receive
                if ((response?.Messages?.Count ?? 0) == 0)
                    yield break;

                commandCount += response.Messages.Count;

                yield return response.Messages.Select(x => new CommandResult<T>()
                {
                    MessageId = x.MessageId,
                    ReceiptHandle = x.ReceiptHandle,
                    Command = DeserializeMessage<T>(x),
                    //CommandDataType = x.Attributes["commandDataType"],
                    ReceiveCount = Convert.ToInt32(x.Attributes["ApproximateReceiveCount"]),
                    Attributes = x.Attributes
                }).ToArray();
            }
        }

        /// <summary>
        /// Remove um lote de comandos da fila após serem processados.
        /// </summary>
        public async Task<ServiceMessage<List<DequeueCommandResult>>> DequeueCommandBatchAsync<T>(ServiceMessage<DequeueCommandBatchMessage<T>> message)
        {
            var result = new ServiceMessage<List<DequeueCommandResult>>(message.Identity, new List<DequeueCommandResult>());
            var commandBatch = message.Data;

            try
            {
                if (commandBatch.Commands.Any())
                {
                    var queueUrl = brokerConfiguration.ResolveQueueUrl(commandBatch);

                    var status = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

                    foreach (var batch in commandBatch.Commands.Chunk(MaxNumberOfMessages))
                    {
                        var entries = batch.Select(x => new DeleteMessageBatchRequestEntry()
                        {
                            Id = x.MessageId,
                            ReceiptHandle = x.ReceiptHandle
                        }).ToList();

                        var response = await amazonSimpleQueueService.DeleteMessageBatchAsync(queueUrl, entries);

                        foreach (var item in response.Successful)
                            status[item.Id] = true;
                        foreach (var item in response.Failed)
                            status[item.Id] = false;
                    }

                    foreach (var msgInfo in commandBatch.Commands)
                    {
                        result.Data.Add(new DequeueCommandResult
                        {
                            MessageId = msgInfo.MessageId,
                            ReceiptHandle = msgInfo.ReceiptHandle,
                            IsSuccess = status[msgInfo.MessageId]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while dequeue messages");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        #region Private Methods

        private T DeserializeMessage<T>(Message brokerMessage)
        {
            dynamic brokerMessageBody = JsonConvert.DeserializeObject(brokerMessage.Body.DecompressFromGzip());
            string message = Convert.ToString(brokerMessageBody.Message) ?? Convert.ToString(brokerMessageBody);
            return JsonConvert.DeserializeObject<T>(message.DecompressFromGzip());
        }

        #endregion
    }
}
