using System;
using System.Net.Http.Formatting;
using System.Text.Json;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order;
using Yah.Hub.Common.Security;

namespace Yah.Hub.Application.Clients.ExternalClient
{
    // TODO: change this to external client and build endpoints by configurations
	public class ERPClient : AbstractHttpClient, IERPClient
	{
        private string BaseUri { get; set; } = "http://a8c5b9bb8adb64666bd6f067a6b8fb5c-1892383710.us-east-1.elb.amazonaws.com/erp/servico-externo";
        private string OrderPath { get; set; } = "pedido";

        public override string GetBaseUri()
        {
            return this.BaseUri;
        }

        

        private JsonMediaTypeFormatter GetFormatter()
        {
            var formatter = new JsonMediaTypeFormatter()
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                    
                }
            };

            formatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return formatter;
        }

        public ERPClient(HttpClient client, ILogger<ERPClient> logger, IConfiguration configuration, IThrottlingService throttlingService) : base(client, logger, configuration, throttlingService)
		{
		}

        public async Task<HttpMarketplaceMessage> SendOrderAsync(MarketplaceServiceMessage<Order> message)
        {
            #region Code

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);
            var request = new HttpRequestMessage(HttpMethod.Post, $"pedido");

            message.Data.TenantId = message.Identity.GetTenantId();
            message.Data.AccountId = message.Identity.GetAccountId();

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(new Dictionary<string, string>() { { "X-tenant-id", message.Identity.GetTenantId() }, { "token", message.Identity.GetTenantId() } });
            try
            {
                var requestResult = ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message)).GetAwaiter().GetResult();

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                    result.MergeHttpResponseMessage(requestResult);
                else
                    result.WithError(new Error(requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult(),"", ErrorType.Technical));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while send order to platform, orderid: {message.Data.OrderId}");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public override string FormatThrottlingKey(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            throw new NotImplementedException();
        }

        public override int? GetRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            throw new NotImplementedException();
        }

        public override bool UseThrottlingControl()
        {
            return false;
        }
    }
}

