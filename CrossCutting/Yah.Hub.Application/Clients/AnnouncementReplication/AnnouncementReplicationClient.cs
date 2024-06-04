using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Clients.AnnouncementReplication.Interface;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Announcement;

namespace Yah.Hub.Common.Clients.AnnouncementReplication
{
    public class AnnouncementReplicationClient : AbstractHttpClient, IAnnouncementReplicationClient
    {
        public AnnouncementReplicationClient(HttpClient httpClient, ILogger logger, IConfiguration configuration, IThrottlingService throttling) : base(httpClient, logger, configuration, throttling)
        {
        }

        public override string FormatThrottlingKey(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            throw new NotImplementedException();
        }

        public override int? GetRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpMarketplaceMessage> ReplicateAnnouncementAsync(MarketplaceServiceMessage<Announcement> message)
        {
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-VendorID", $"{message.Data.VendorId}" },
                    { "x-TenantId", $"{message.Data.TenantId}" },
                    { "x-AccountId", $"{message.Data.AccountId}" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri("", UriKind.Relative));
                request.SetJsonContent(message.Data);
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(message));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                    result.MergeHttpResponseMessage(requestResult);
                else
                    result.WithError( new Error("Erro na tentativa de replicação do anúncio para o ElasticSearch", "Erro na tentativa de replicação do anúncio para o ElasticSearch", ErrorType.Technical));

                return result;

            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
                return result;
            }
        }

        public override bool UseThrottlingControl()
        {
            return false;
        }
    }
}
