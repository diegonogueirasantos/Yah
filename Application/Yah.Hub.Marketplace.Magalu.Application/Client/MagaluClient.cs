using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Magalu.Application.Models;
using Yah.Hub.Marketplace.Magalu.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Models.Order;
using Nest;
using Yah.Hub.Common.Extensions;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Dynamic;
using Yah.Hub.Domain.Catalog;
using Product = Yah.Hub.Marketplace.Magalu.Application.Models.Product;
using Sku = Yah.Hub.Marketplace.Magalu.Application.Models.Sku;
using Price = Yah.Hub.Marketplace.Magalu.Application.Models.Price;
using Category = Yah.Hub.Marketplace.Magalu.Application.Models.Category;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Token = Yah.Hub.Marketplace.Magalu.Application.Models.Token;
using CsvHelper.Configuration;
using System;

namespace Yah.Hub.Marketplace.Magalu.Application.Client
{
    public class MagaluClient : AbstractHttpClient, IMagaluClient
    {
        public MagaluClient(HttpClient httpClient, ILogger<MagaluClient> logger, IConfiguration configuration, IThrottlingService throttlingService) : base(httpClient, logger, configuration, throttlingService)
        {
        }

        #region [Authentication]

        public async Task<HttpMarketplaceMessage> ValidateAuthorization(MarketplaceServiceMessage message)
        {
            #region [Code]

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/EndpointLimit", UriKind.Relative));

            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Configuration).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));

                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<ResponseToken>> AuthenticationTokenAsync(MarketplaceServiceMessage<Dictionary<string, string>> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<ResponseToken>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://id.magalu.com/oauth/token", UriKind.Absolute));

            request.SetEncodedContent(message.Data);

            var requestResult = await ExecuteCustomRequest((request, EntityType.Configuration).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                var buffer = await requestResult.Content.ReadAsByteArrayAsync();
                var responseString = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                result.WithError(new Error(responseString, responseString, ErrorType.Business));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> FinalizedAuthenticationAsync(MarketplaceServiceMessage message)
        {
            #region Code

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("oauth2/rollout/finalized", UriKind.Relative));

            var requestResult = await ExecuteRequest((request, EntityType.Configuration).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        #endregion

        #region [Catalog]

        public async Task<HttpMarketplaceMessage> CreateProduct(MarketplaceServiceMessage<Product> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));
            
            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("api/Product", UriKind.Relative));
            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage>CreateSku(MarketplaceServiceMessage<Sku> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/Sku", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Sku).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateProduct(MarketplaceServiceMessage<Product> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("/api/Product", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateSku(MarketplaceServiceMessage<Sku> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("/api/Sku", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Sku).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdatePrice(MarketplaceServiceMessage<Price[]> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("/api/Price", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Price).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> UpdateStock(MarketplaceServiceMessage<Stock[]> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri("/api/Stock", UriKind.Relative));

            request.SetJsonContent(message.Data, GetFormatter());
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Inventory).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<Product>> GetProduct(MarketplaceServiceMessage<string> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<Product>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"/api/Product/{message.Data}", UriKind.Relative));

            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<Sku>> GetSku(MarketplaceServiceMessage<string> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage<Sku>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"/api/Sku/{message.Data}", UriKind.Relative));

            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Sku).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> CreateCategory(MarketplaceServiceMessage<Category[]> message)
        {
            #region Code

            if (message.Data == null)
                throw new ArgumentNullException(nameof(message.Data));

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/Category", UriKind.Relative));
            request.SetJsonContent(message.Data);
            request.SetHeaders(GetHeaders(message));

            var requestResult = await ExecuteRequest((request, EntityType.Category).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult);

            if (!requestResult.IsSuccessStatusCode)
            {
                result.WithErrors(await this.TryReadErrors(requestResult));
                return result;
            }

            return result;

            #endregion
        }

        #endregion

        #region [Order]

        public async Task<HttpMarketplaceMessage<MagaluOrderQueue>> GetOrdersFromQueue(MarketplaceServiceMessage<string> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<MagaluOrderQueue>(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/OrderQueue?Status={message.Data}");
                request.SetHeaders(GetHeaders(message));

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    result.WithErrors(await this.TryReadErrors(requestResult));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<MagaluOrder>> GetOrder(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<MagaluOrder>(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Order/{message.Data.IntegrationOrderId}");
                request.SetHeaders(GetHeaders(message));

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    result.WithErrors(await this.TryReadErrors(requestResult));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> DequeueOrders(MarketplaceServiceMessage<OrderQueueItemId[]> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "/api/OrderQueue");
                request.SetHeaders(GetHeaders(message));
                request.SetJsonContent(message.Data);

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (!requestResult.IsSuccessStatusCode)
                {
                    var content = await requestResult.Content.ReadAsStringAsync();

                    if (content.Contains("Item já confirmado", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        return result;
                    }

                    result.WithError(new Error($"Error ao remover pedidos da fila do marketplace {MarketplaceAlias.Magalu}", requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult(), ErrorType.Technical));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<MagaluInvoiceOrder> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "/api/Order");
                request.SetHeaders(GetHeaders(message));
                request.SetJsonContent(message.Data);

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (!requestResult.IsSuccessStatusCode)
                {
                    result.WithError(new Error($"Error ao atualizar o pedido {message.Data.IdOrder} para Faturado no marketplace {MarketplaceAlias.Magalu}", requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult(), ErrorType.Technical));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> ShipmentOrder(MarketplaceServiceMessage<MagaluShipOrder> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "/api/Order");
                request.SetHeaders(GetHeaders(message));
                request.SetJsonContent(message.Data);

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (!requestResult.IsSuccessStatusCode)
                {
                    result.WithError(new Error($"Error ao atualizar o pedido {message.Data.IdOrder} para Enviado no marketplace {MarketplaceAlias.Magalu}", requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult(), ErrorType.Technical));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> DeliveryOrder(MarketplaceServiceMessage<MagaluDeliveryOrder> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "/api/Order");
                request.SetHeaders(GetHeaders(message));
                request.SetJsonContent(message.Data);

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (!requestResult.IsSuccessStatusCode)
                {
                    result.WithError(new Error($"Error ao atualizar o pedido {message.Data.IdOrder} para Entregue no marketplace {MarketplaceAlias.Magalu}", requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult(), ErrorType.Technical));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> ProcessingOrder(MarketplaceServiceMessage<MagaluOrderStatus> message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "/api/Order");
                request.SetHeaders(GetHeaders(message));
                request.SetJsonContent(message.Data);

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (!requestResult.IsSuccessStatusCode)
                {
                    result.WithError(new Error($"Error ao atualizar o pedido {message.Data.IdOrder} para Faturado no marketplace {MarketplaceAlias.Magalu}", requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult(), ErrorType.Technical));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<MagaluShipmentLabel[]>> GetShipmentLabel(MarketplaceServiceMessage<MagaluShipmentLabelRequest> message) 
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<MagaluShipmentLabel[]>(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/Order/ShippingLabels");
                request.SetHeaders(GetHeaders(message));
                request.SetJsonContent(message.Data);

                var requestResult = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(message));

                if (requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    result.WithErrors(await this.TryReadErrors(requestResult));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        #endregion

        #region [Throttle]

        public async Task<HttpMarketplaceMessage<List<MagaluApiLimit>>> GetApiLimit(MarketplaceServiceMessage message)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<List<MagaluApiLimit>>(message.Identity, message.AccountConfiguration);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/EndPointLimit");
                request.SetHeaders(GetHeaders(message));

                var requestResult = await this.ExecuteRequest((request, EntityType.Configuration).AsMarketplaceServiceMessage(message));

                if (requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    result.WithErrors(await this.TryReadErrors(requestResult));
                }
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }

        public override bool UseThrottlingControl()
        {
            return true;
        }

        public override string FormatThrottlingKey(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            var message = marketplaceServiceMessage.Data.requestMessage;
            var entityType = marketplaceServiceMessage.Data.entityType;
            var method = marketplaceServiceMessage.Data.requestMessage.Method.ToString();
            string path;

            if (message.Method.Equals(HttpMethod.Get) && entityType.In(EntityType.Product, EntityType.Sku))
            {
                path = $"{String.Join("/", marketplaceServiceMessage.Data.requestMessage.RequestUri.ToString().Split("/").SkipLast(1))}/{{id}}".Trim();
            }
            else
            {
                path = marketplaceServiceMessage.Data.requestMessage.RequestUri.ToString();
            }

            var lockKey = $"({method}) {path}";

            return lockKey;
        }

        public override int? GetRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            var lockKey = this.FormatThrottlingKey(marketplaceServiceMessage);

            var resultApiLimite = this.GetApiLimit(marketplaceServiceMessage).GetAwaiter().GetResult();

            var rateLimite = resultApiLimite.Data.Where(x => x.Name.Equals(lockKey))?.FirstOrDefault()?.RequestsByMinute ?? null;

            return rateLimite;
        }

        #endregion


        #region Private Methods

        private async Task<List<Error>> TryReadErrors(HttpResponseMessage response)
        {
            var list = new List<Error>();

            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    list.Add(new Error($"StatusCode: {response.StatusCode}, acesso não autorizado.","",ErrorType.Technical));
                    return list;
                }

                var error = await response.Content.ReadAsAsync<MagaluError>();
                if (error.Errors?.Count > 0)
                    list.AddRange(error.Errors.Select(x => new Error(x.Message,"",ErrorType.Business)));
                else
                    list.Add(new Error($"{error.Message}",$"{error.Description}",ErrorType.Business));
            }
            catch (Exception ex)
            {
                list.Add(new Error(ex));
            }

            return list;
        }

        private Dictionary<string, string> GetHeaders(MarketplaceServiceMessage message)
        {
            var result = new Dictionary<string, string>();
            result.Add("Accept", "application/json");
            result.Add("Authorization", $"Bearer {message.AccountConfiguration.AccessToken}");
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

        #endregion
    }
}
