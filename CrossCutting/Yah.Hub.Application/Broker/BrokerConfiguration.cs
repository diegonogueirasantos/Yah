using Yah.Hub.Application.Broker.Messages;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Services;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;


namespace Yah.Hub.Marketplace.Application.Broker
{
    public class BrokerConfiguration : AbstractService, IBrokerConfiguration
    {
        protected readonly IConfiguration configuration;
       
        #region Constructor

        public BrokerConfiguration(IConfiguration configuration, ILogger<BrokerConfiguration> logger) : base(configuration, logger)
        {
            this.configuration = configuration;
        }

        #endregion Constructor

        #region Public methods

        public virtual string ResolveTopic<T>(BrokerBaseEntity<T> message)
        {
            var arn = GetSetting(message, "sns:arn"); 
            var topic = GetSetting(message, "sns:topic");
            topic = topic?
                .Replace("{environment}", this.GetEnvironment().ToUpper(), StringComparison.InvariantCultureIgnoreCase)
                .ToUpper();
            topic = topic?
               .Replace("{marketplace}", message.Marketplace.ToString().ToUpper(), StringComparison.InvariantCultureIgnoreCase)
               .ToUpper();
            return $"{arn}{topic}";
        }

        public virtual string ResolveSubject<T>(BrokerBaseEntity<T> message)
        {
            return GetSetting(message, "sqs:messageSubject");
        }

        public virtual string ResolveQueueUrl<T>(T message)
        {
            var url = GetSetting(message, "sqs:url");
            var queue = GetSetting(message, "sqs:queue");
            queue = queue
                .Replace("{environment}", this.GetEnvironment(), StringComparison.InvariantCultureIgnoreCase)
                .ToUpper();
            return $"{url}{queue}";
        }

        public virtual string ResolveDLQueueUrl<T>(BrokerBaseEntity<T> message)
        {
            var url = GetSetting(message, "sqs:url");
            var queue = GetSetting(message, "sqs:dl_queue");
            queue = queue
                .Replace("{environment}", this.GetEnvironment(), StringComparison.InvariantCultureIgnoreCase)
                .ToUpper();
            return $"{url}{queue}";
        }

        public virtual int ResolveBatchSize<T>(T message)
        {
            return GetSettingAsInt32<T>(message, "sqs:maxNumberOfMessages");
        }

        public virtual int ResolveVisibility<T>(T message)
        {
            return GetSettingAsInt32<T>(message, "sqs:visibilityTimeout");
        }

        #endregion Public methods

        #region Private methods

        private string GetBrokerName<T>(T commandMessage) 
        {
            return typeof(T)?.GenericTypeArguments?.FirstOrDefault()?.Name?.ToLowerInvariant().RemoveAccents().OnlyLetters() ?? typeof(T).Name;
        }

        private string GetRootConfiguration<T>(T commandMessage)
        {
            return $"aws:brokers:{this.GetBrokerName<T>(commandMessage)}";
        }

        protected int GetSettingAsInt32<T>(T message, string settingEntry)
        {
            Int32.TryParse(GetSetting<T>(message, settingEntry), out int result);
            return result;
        }

        protected bool GetSettingAsBool<T>(T message, string settingEntry)
        {
            bool.TryParse(GetSetting<T>(message, settingEntry), out bool result);
            return result;
        }

        protected string GetSetting<T>(T message, string settingEntry)
        {
            return configuration[$"{this.GetRootConfiguration<T>(message)}:{settingEntry}"];
        }

        #endregion Private methods
    }
}
