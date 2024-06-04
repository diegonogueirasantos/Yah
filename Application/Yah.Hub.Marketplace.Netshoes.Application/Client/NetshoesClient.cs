using Newtonsoft.Json;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Marketplace.Netshoes.Application.Models;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Errors;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Order;
using Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus;
using System.Dynamic;
using System.Net.Http.Formatting;
using Error = Yah.Hub.Common.ServiceMessage.Error;

namespace Yah.Hub.Marketplace.Netshoes.Application.Client
{
    public class NetshoesClient : AbstractHttpClient, INetshoesClient
    {
        public NetshoesClient(HttpClient httpClient, ILogger<NetshoesClient> logger, IConfiguration configuration, IThrottlingService throttling) : base(httpClient, logger, configuration, throttling)
        {
        }

        public Dictionary<string, string> GetAuthentication(MarketplaceServiceMessage message)
        {
            return new Dictionary<string, string>() { { "client_id", Configuration["Marketplace:ClientId"] }, { "access_token", message.AccountConfiguration.AccessToken } };
        }

        #region [Authentication]

        public async Task<HttpMarketplaceMessage> TryAuthenticate(MarketplaceServiceMessage message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v1/sellers", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error("Erro ao autenticar as credênciais no marketplace", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }

        #endregion

        #region [Catalog]
        public async Task<HttpMarketplaceMessage<Product>> GetProduct(MarketplaceServiceMessage<string> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<Product>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v2/products/{message.Data}/status", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error("Erro ao recuperar dados de produto.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> CreateProduct(MarketplaceServiceMessage<Product> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("v2/products", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateProduct(MarketplaceServiceMessage<Product> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"v2/products/{message.Data.Sku}", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<Price>> GetPrice(MarketplaceServiceMessage<string> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<Price>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v2/products/{message.Data}/prices", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
                return result;
            }

            var error = new Error("Erro ao recuperar dados do preço do produto.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical);

            result.WithError(error);

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> CreatePrice(MarketplaceServiceMessage<(Price Price, string Sku)> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"v2/products/{message.Data.Sku}/prices", UriKind.Relative));

            request.SetJsonContent(message.Data.Price, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdatePrice(MarketplaceServiceMessage<(Price Price, string Sku)> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"v2/products/{message.Data.Sku}/prices", UriKind.Relative));

            request.SetJsonContent(message.Data.Price, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<Stock>> GetStock(MarketplaceServiceMessage<string> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<Stock>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v2/products/{message.Data}/stocks", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
                return result;
            }

            var error = new Error("Erro ao recuperar dados do estoque do produto.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical);

            result.WithError(error);

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> CreateStock(MarketplaceServiceMessage<(Stock Stock, string Sku)> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"v2/products/{message.Data.Sku}/stocks", UriKind.Relative));

            request.SetJsonContent(message.Data.Stock, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateStock(MarketplaceServiceMessage<(Stock Stock, string Sku)> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"v2/products/{message.Data.Sku}/stocks", UriKind.Relative));

            request.SetJsonContent(message.Data.Stock, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        #endregion

        #region [Order]

        public async Task<HttpMarketplaceMessage<Order>> GetOrder(MarketplaceServiceMessage<string> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<Order>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v1/orders/{message.Data}?expand=shippings,items", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error("Erro ao recuperar dados do pedido.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<OrderResult>> GetOrders(MarketplaceServiceMessage<SearchOrders> message)
        {
            #region [code]

            var result = new HttpMarketplaceMessage<OrderResult>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v1/orders?page={message.Data.Page}&size={message.Data.Size}&orderStartDate={message.Data.From}&orderEndDate={message.Data.To}&expand=shippings,items", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error("Erro ao recuperar dados dos pedidos.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> ChangeOrderStatus<T>(MarketplaceServiceMessage<OrderStatusWrapper<T>> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"v1/orders/{message.Data.IntegrationOrderId}/shippings/{message.Data.Code}/status/{message.Data.Status.ToLower()}", UriKind.Relative));

            request.SetJsonContent(message.Data.Data, GetFormatter());
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ProductErrors>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<ShipmentLabelResult>> GetShipmentLabel(MarketplaceServiceMessage<ShipmentLabelRequest> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<ShipmentLabelResult>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"v1/orders/shipping-tags", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));
            request.SetJsonContent(message.Data);

            var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

            if (requestResult != null && requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                var resultErrors = await requestResult.TryReadAsAsync<ShipmentLabelError>(message);

                result.WithErrors(this.NetshoesErrors(resultErrors.Data));
            }

            return result;
            #endregion
        }

        #endregion

        #region [Category]

        public async Task<HttpMarketplaceMessage<CategoriesTree>> GetCategories(MarketplaceServiceMessage<string?> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<CategoriesTree>(message.Identity, message.AccountConfiguration);

            string url = String.IsNullOrEmpty(message.Data) ? "v1/bus/NS/departments" : $"v1/department/{message.Data}/productType";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Category).AsMarketplaceServiceMessage(message));

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error("Erro ao recuperar dados da categoria.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<AttributeGroup>> GetAttributes(MarketplaceServiceMessage<(string categoryId, string attributeID)> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<AttributeGroup>(message.Identity, message.AccountConfiguration);

            string url = $"v1/department/{message.Data.categoryId}/productType/{message.Data.attributeID}/templates";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url, UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Category).AsMarketplaceServiceMessage(message));

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error("Erro ao recuperar dados de atributos.", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }

        #endregion

        #region [Product Templates]

        public async Task<HttpMarketplaceMessage<GenericAttributes>> GetTemplateAttributes(MarketplaceServiceMessage<string> message)
        {
            #region [code]
            var result = new HttpMarketplaceMessage<GenericAttributes>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"v1/{message.Data}", UriKind.Relative));

            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Category).AsMarketplaceServiceMessage(message));

            if (requestResult.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(requestResult);
            }
            else
            {
                result.StatusCode = requestResult.StatusCode;
                result.WithError(new Error($"Erro ao recuperar dados de {message.Data}", requestResult.IsSuccessStatusCode.ToString(), ErrorType.Technical));
            }

            return result;
            #endregion
        }
        #endregion


        #region [Private]
        private List<Error> NetshoesErrors(ProductErrors errors)
        {
            #region [code]
            if (errors == null || !errors.Errors.Any())
            {
                return new List<Error>() { new Error("Erro ao desconhecido Netshoes.","",ErrorType.Technical) };
            }

            var resultErrors = new List<Error>();

            foreach (var error in errors.Errors)
            {
                resultErrors.Add(new Error(error,"",ErrorType.Business));
            }

            return resultErrors;
            #endregion
        }
        private List<Error> NetshoesErrors(ShipmentLabelError errors)
        {
            #region [code]
            if (errors == null || !errors.Errors.Any())
            {
                return new List<Error>() { new Error("Erro desconhecido ao tentar obter a etiqueta do pedido na Netshoes.", "", ErrorType.Technical) };
            }

            var resultErrors = new List<Error>();

            foreach (var error in errors.Errors)
            {
                resultErrors.Add(new Error(error.Description, $"erro Code {error.Code}", ErrorType.Business));
            }

            return resultErrors;
            #endregion
        }

        private JsonMediaTypeFormatter GetFormatter()
        {
            var formatter = new JsonMediaTypeFormatter()
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                }
            };

            formatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return formatter;
        }
        #endregion

        #region [Throttle]
        public override int? GetRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            return 100;
        }

        public override bool UseThrottlingControl()
        {
            return false;
        }

        public override string FormatThrottlingKey(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            return $"{marketplaceServiceMessage.Data.requestMessage.Method.ToString()}";
        }
        #endregion
    }
}
