using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Clients.AnnouncementReplication.Interface;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.Security.Identities;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Monitor;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;
using static System.Net.Mime.MediaTypeNames;
using Identity = Yah.Hub.Common.Identity.Identity;

namespace Yah.Hub.Functions.ReplicationAnnouncement.Service
{
    public class ReplicationAnnouncementHandler : AbstractService, IReplicationAnnouncementHandler
    {
        private IAnnouncementReplicationClient Client { get; }
        protected IAccountConfigurationService ConfigurationService { get; }

        public ReplicationAnnouncementHandler(
            IAnnouncementReplicationClient Client,
            IConfiguration configuration,
            ILogger logger) : base(configuration, logger)
        {
            this.Client = Client;
        }

        public async Task<ServiceMessage> ReplicateAnnouncement(DynamodbStreamRecord record)
        {
            var hasNewImage = (record.Dynamodb.NewImage?.Any()).GetValueOrDefault();
            var image = hasNewImage ? record.Dynamodb.NewImage : record.Dynamodb.OldImage;

            image.TryGetValue("Data", out AttributeValue dataValue);
            
            var identity = this.GetIdentity(image);
            
            identity.IsValidVendorTenantAccountIdentity();

            var result = new ServiceMessage(identity);

            var announcement = JsonConvert.DeserializeObject<Announcement>(dataValue.S);

            var resultRequest = await this.Client.ReplicateAnnouncementAsync(announcement.AsMarketplaceServiceMessage<Announcement>(identity, announcement.Marketplace));

            if (!resultRequest.IsValid)
            {
                result.WithErrors(resultRequest.Errors);
                return result;
            }

            return result;
        }

        private Identity GetIdentity(Dictionary<string, AttributeValue> message)
        {
            message.TryGetValue("PartitionKey", out AttributeValue partitionValue);
            message.TryGetValue("accountId", out AttributeValue accountValue);

            return new VendorTenantAccountIdentity(partitionValue.S.Split('-').Last(), partitionValue.S.Split('-').First(), accountValue.S);
        }
    }
}
