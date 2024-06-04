using Newtonsoft.Json;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Client.Interface;
using System.Dynamic;
using System.Net.Http.Formatting;
using Yah.Hub.Marketplace.B2W.Application.Models.Order;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Amazon.Runtime.Internal.Transform;
using Elasticsearch.Net;
using HttpMethod = System.Net.Http.HttpMethod;
using Error = Yah.Hub.Common.ServiceMessage.Error;
using System.Linq;
using Nest;

namespace Yah.Hub.Marketplace.B2W.Application.Client
{
    public class B2WClient : AbstractHttpClient, IB2WClient
    {
        private const string ApiProductionUrl = "https://api.skyhub.com.br/";
        public readonly string queueUri = "/queues";
        public readonly string AccountManagerKey = "qCkizR6opd";

        public Dictionary<string, string> GetAuthentication(MarketplaceServiceMessage message)
        {
            return new Dictionary<string, string>() { { "X-User-Email", message.AccountConfiguration.Email }, { "X-Api-Key", message.AccountConfiguration.AccessToken }, { "X-Accountmanager-Key", AccountManagerKey }, { "Authorization", $"Bearer {message.AccountConfiguration.RefreshToken}" } };
        }

        public B2WClient(HttpClient httpClient, ILogger<B2WClient> logger, IConfiguration configuration, IThrottlingService throttlingService) : base(httpClient, logger, configuration, throttlingService)
        {
        }

        #region Product

        public async Task<HttpMarketplaceMessage> CreateProduct(MarketplaceServiceMessage<Product> message)
        {
            var wrapper = new ProductWrapper { Product = message.Data };

            var result = new HttpMarketplaceMessage<ExpandoObject, ExpandoObject>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("products", UriKind.Relative));
            request.SetJsonContent(wrapper);
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            return result;
        }

        public async Task<HttpMarketplaceMessage> UpdateProduct(MarketplaceServiceMessage<Product> message)
        {
            var wrapper = new ProductWrapper { Product = message.Data };

            var result = new HttpMarketplaceMessage<ExpandoObject, ExpandoObject>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"products/{message.Data.Sku}", UriKind.Relative));
            request.SetJsonContent(wrapper);
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            return result;
        }

        public async Task<HttpMarketplaceMessage<Product>> GetProduct(MarketplaceServiceMessage<string> message)
        {
            var result = new HttpMarketplaceMessage<Product, ExpandoObject>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"products/{message.Data}", UriKind.Relative));
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Product).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            return result;
        }

        public async Task<HttpMarketplaceMessage> UpdateVariation(MarketplaceServiceMessage<Variation> message)
        {
            var wrapper = new VariationWrapper() { Variation = message.Data};

            var result = new HttpMarketplaceMessage<ExpandoObject, ExpandoObject>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri($"variations/{message.Data.Sku}", UriKind.Relative));
            request.SetJsonContent(wrapper);
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Sku).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            return result;
        }

        public async Task<HttpMarketplaceMessage<B2WErrors>> GetProductErrors(MarketplaceServiceMessage<string> message)
        {
            var result = new HttpMarketplaceMessage<B2WErrors, ExpandoObject>(message.Identity, message.AccountConfiguration);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"/sync_errors/products?entity_id={message.Data}", UriKind.Relative));
            request.SetHeaders(GetAuthentication(result));

            var requestResult = await ExecuteRequest((request, EntityType.Configuration).AsMarketplaceServiceMessage(message));

            result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            return result;
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

        #region Order

        public async Task<HttpMarketplaceMessage<B2WOrder>> GetOrderFromQueue(MarketplaceServiceMessage marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<B2WOrder, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);
            
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/queues/orders");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        

        public async Task<HttpMarketplaceMessage<B2WOrder>> GetOrder(MarketplaceServiceMessage marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<B2WOrder, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/queues/orders");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<Shipment>> GetOrderShipment(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<Shipment, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/shipments/b2w?delivery_id={marketplaceServiceMessage.Data}");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        /// <summary>
        /// TODO: ESTE MÉTODO PELA DOCUMENTAÇÃO RETORNA APENAS UMA MENSAGEM COM O ID DA PLP DENTRO DELA EM UM FORMATO STRING, VERIFICAR SE CONDIZ COM A REALIDADE
        /// </summary>
        /// <param name="marketplaceServiceMessage"></param>
        /// <returns></returns>
        public async Task<HttpMarketplaceMessage<string>> GroupOrderShipments(MarketplaceServiceMessage<PlpGroup> marketplaceServiceMessage)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, $"/shipments/b2w");
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            ///
            #endregion
        }

        
        public async Task<HttpMarketplaceMessage<byte[]>> GetShipmentLabel(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]

            var result = new HttpMarketplaceMessage<byte[], ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"shipments/b2w/view?plp_id={marketplaceServiceMessage.Data}");
                httpRequest.Headers.Accept.Clear();
                httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/pdf"));
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            ///shipments/b2w/view?plp_id={CODE}
            #endregion
        }



        public async Task<HttpMarketplaceMessage> TryInvoiceOrder(MarketplaceServiceMessage<B2WInvoiceOrder> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/orders/{marketplaceServiceMessage.Data.OrderId}/invoice");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while invoice order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> TryInvoiceXMLOrder(MarketplaceServiceMessage<(MultipartFormDataContent contentXML, string orderId)> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/orders/{marketplaceServiceMessage.Data.orderId}/invoice");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetXMLContent(marketplaceServiceMessage.Data.contentXML);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while invoice order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> TryShipOrder(MarketplaceServiceMessage<B2WShipOrder> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/orders/{marketplaceServiceMessage.Data.OrderId}/shipments");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while ship order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> TryDeliveryOrder(MarketplaceServiceMessage<B2WDeliveryOrder> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/orders/{marketplaceServiceMessage.Data.OrderId}/delivery");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while delivey order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> TryCancelOrder(MarketplaceServiceMessage<B2WCancelOrder> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/orders/{marketplaceServiceMessage.Data.OrderId}/cancel");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while try cancel order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> TryShipExceptionOrder(MarketplaceServiceMessage<B2WShipExceptionOrder> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/orders/{marketplaceServiceMessage.Data.OrderId}/shipment_exception");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while try Shipment Exceptio order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> TryDequeueOrder(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<string, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/queues/orders/{marketplaceServiceMessage.Data}");
                httpRequest.SetHeaders(this.GetAuthentication(marketplaceServiceMessage));

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));
                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while try to dequeue order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public override bool UseThrottlingControl()
        {
            return false;
        }

        public override string FormatThrottlingKey(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            throw new NotImplementedException();
        }

        public override int? GetRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpMarketplaceMessage<AuthResponse>> RenewRehubToken(MarketplaceServiceMessage<Auth> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<AuthResponse, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/auth");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Configuration).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<ProductLinkResult>> LinkProduct(MarketplaceServiceMessage<ProductLink> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<ProductLinkResult, ExpandoObject>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/rehub/product_actions");
                httpRequest.SetHeaders(GetAuthentication(marketplaceServiceMessage));
                httpRequest.SetJsonContent(marketplaceServiceMessage.Data);

                var requestResult = await this.ExecuteRequest((httpRequest, EntityType.Configuration).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult, MapErrors, GetFormatter());
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        #region Private

        public List<Error> MapErrors(ExpandoObject error)
        {
            var errorMessage = error.GetPropertyValue<string>("error");
            var errors = new List<Error>();

            if (!String.IsNullOrEmpty(errorMessage))
                errors.Add(new Error(errorMessage, errorMessage, ErrorType.Business));

            return errors;
        }

        #endregion
    }
}
