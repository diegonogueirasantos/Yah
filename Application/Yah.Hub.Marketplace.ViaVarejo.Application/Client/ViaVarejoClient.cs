using Newtonsoft.Json;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Category;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order;
using System.Net.Http.Formatting;
using System.Text;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Client
{
    public class ViaVarejoClient : AbstractHttpClient, IViaVarejoClient
    {
        public ViaVarejoClient(HttpClient httpClient, ILogger<ViaVarejoClient> logger, IConfiguration configuration, IThrottlingService throttling) : base(httpClient, logger, configuration, throttling)
        {
        }

        #region [Validation]

        public async Task<HttpMarketplaceMessage> ValidateCredentials(MarketplaceServiceMessage message)
        {
            #region Code

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("v4/api-front-categories-v3/jersey/categories?_offset=0&_limit=1", UriKind.Relative));
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        #endregion

        #region [Catalog]
        public async Task<HttpMarketplaceMessage> UpsertProduct(MarketplaceServiceMessage<ProductWrapper> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("v4/sandbox/api-front-importer-v4/jersey/import/itens", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if(result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<Product>> GetSkuStatus(MarketplaceServiceMessage<(string productId, string skuId)> message)
        {
            #region Code

            if (message.Data.productId == null || message.Data.skuId == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<Product>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v4/sandbox/api-front-importer-v4/jersey/import/itens/{message.Data.productId}/sku/{message.Data.skuId}/status", UriKind.Relative));
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> DeleteProduct(MarketplaceServiceMessage<(string productId, string skuId)> message)
        {
            #region Code

            if (message.Data.productId == null && message.Data.skuId == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri($"v4/api-front-importer-v4/jersey/import/itens/{message.Data.productId}/sku/{message.Data.skuId}", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdatePrice(MarketplaceServiceMessage<PriceUpdate> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("v4/api-front-offer-v4/jersey/offer/price", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateInventory(MarketplaceServiceMessage<InventoryUpdate> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("v4/api-front-offer-v4/jersey/offer/stock", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> SetSkuOption(MarketplaceServiceMessage<SkuOption> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"v4/api-front-importer-v4/jersey/import/itens/{message.Data.ProductId}/sku/{message.Data.SelectedSku}/optar", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);

            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateProductStatus(MarketplaceServiceMessage<UpdateStatus> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("v4/sandbox/api-front-offer-v4/jersey/offer/status", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        #endregion

        #region [Category]
        public async Task<HttpMarketplaceMessage<CategoryTree>> GetCategories(MarketplaceServiceMessage<SearchCategory> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<CategoryTree>(message.Identity, message.AccountConfiguration);
            
            
            
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v4/api-front-categories-v3/jersey/categories?{this.QueryStringBuilder(message.Data)}", UriKind.Relative));
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        #endregion

        #region [Order]
        public async Task<HttpMarketplaceMessage<OrderResult>> GetOrders(MarketplaceServiceMessage<SearchOrders> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<OrderResult>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v2/orders/sandbox/status/{message.Data.Status}?{this.QueryStringBuilder(message.Data)}", UriKind.Relative));
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<Order>> GetOrder(MarketplaceServiceMessage<string> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<Order>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v2/sandbox/orders/{message.Data}", UriKind.Relative));
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
                else
                    result.WithError(new Error(result.StatusCode == System.Net.HttpStatusCode.NotFound ? "Order not found" : "Error while get order from marketplace", "Order Error", ErrorType.Business));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> SetOrderStatus<T>(MarketplaceServiceMessage<UpdateOrderStatus<T>> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"v2/sandbox/orders/{message.Data.OrderId}/trackings/{message.Data.Status}", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<ShipmentLabel>> GetShipmentLabel(MarketplaceServiceMessage<ShipmentLabelRequest> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<ShipmentLabel>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"v2/sandbox/orders/{message.Data.OrderId}/generate-label", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                if (result.Errors.Any())
                    result.WithErrors(result.Errors);
            }

            return result;

            #endregion
        }
        #endregion

        #region [Throttle]
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
        #endregion

        #region [Private]

        private Dictionary<string, string> GetHeaders(MarketplaceServiceMessage message)
        {
            var result = new Dictionary<string, string>();
            result.Add("Accept", "application/json");
            result.Add("access_token", message.AccountConfiguration.AccessToken);
            result.Add("client_id", message.AccountConfiguration.AppId);

            return result;
        }

        private JsonMediaTypeFormatter GetFormatter()
        {
            #region Code

            var formatter = new JsonMediaTypeFormatter()
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Culture = new System.Globalization.CultureInfo("pt-BR")
                }
            };

            formatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return formatter;

            #endregion
        }

        private string QueryStringBuilder(SearchCategory message)
        {
            if (!String.IsNullOrEmpty(message.Id))
            {
                return $"id={message.Id}";
            }

            return $"_offset={message.Offset}&_limit={message.Limit}";
        }

        private string QueryStringBuilder(SearchOrders message)
        {
            return $"_offset={message.Offset}&_limit={message.Limit}&purchasedAt={message.From},{message.To}";
        }
        #endregion
    }
}
