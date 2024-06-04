using Amazon.Runtime.Internal.Transform;
using Nest;
using Newtonsoft.Json;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Category;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.ShipmentLabel;
using System.Dynamic;
using System.Net.Http.Formatting;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient
{
    public class BrokenClient : IBrokenClient
    {
        IConfiguration Configuration { get; }
        protected HttpClient HttpClient { get; set; }

        public BrokenClient(HttpClient httpClient, ILogger<BrokenClient> logger, IConfiguration configuration)
        {
            Configuration = configuration;
            HttpClient = httpClient;
        }

        public async Task<HttpMarketplaceMessage> RequestMessage<T>(MarketplaceServiceMessage<WrapperRequest<T>> message)
        {
            #region [Code]
            var request = new HttpRequestMessage(message.Data.Method, this.GetUri(message));
            
            var result = new HttpMarketplaceMessage<ServiceMessageResult>(message.Identity, message.AccountConfiguration);

            if (message.Data.Data != null)
            {
                request.SetJsonContent(message.Data.Data);
            }

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult);

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro realizar o envio do {message.Data.Data.GetType().Name} para o marketplace {message.Marketplace.ToString()}", "", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        #region [Category]
        public async Task<HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceCategory>>>> GetCategoryById(MarketplaceServiceMessage<string> message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}GetCategory");

            var result = new HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceCategory>>>(message.Identity, message.AccountConfiguration);

            if (message.Data != null)
            {
                request.SetJsonContent(message.Data);
            }

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult, GetFormatter());

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro realizar a busca da categoria no marketplace {message.Marketplace.ToString()}", $"{result?.Data}", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceCategory>>>> GetCategories(MarketplaceServiceMessage message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}GetCategory");

            var result = new HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceCategory>>>(message.Identity, message.AccountConfiguration);

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult, GetFormatter());

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro realizar a busca da categoria no marketplace {message.Marketplace.ToString()}", $"{result?.Data}", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceAttributes>>>> GetAttributes(MarketplaceServiceMessage<string?> message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}GetAttribute");

            var result = new HttpMarketplaceMessage<ServiceMessageResult<List<MarketplaceAttributes>>>(message.Identity, message.AccountConfiguration);

            if (message.Data != null)
            {
                request.SetJsonContent(message.Data);
            }

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult, GetFormatter());

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro realizar a busca de attributos no marketplace {message.Marketplace.ToString()}", $"{result?.Data}", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }


        #endregion

        #region [Authentication]

        public async Task<HttpMarketplaceMessage> SaveAccountConfigurationSync(MarketplaceServiceMessage<Dictionary<string,string>> message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Post, $"{this.GetUri(message)}SaveAccountConfiguration");

            var result = new HttpMarketplaceMessage<ServiceMessageResult>(message.Identity, message.AccountConfiguration);

            if (message.Data != null)
            {
                request.SetHeaders(message.Data);
            }

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);


            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult);

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro ao salvar as configurações da conta", "", ErrorType.Technical));
                }
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> ValidadeCredentials(MarketplaceServiceMessage message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}ValidadeCredentials");

            var result = new HttpMarketplaceMessage<ServiceMessageResult>(message.Identity, message.AccountConfiguration);

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult);

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Credenciais inválidas para o marketplace {message.Marketplace.ToString()}", "", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        #endregion

        #region [Order]

        public async Task<HttpMarketplaceMessage<ServiceMessageResult<ShipmentLabel>>> GetShipmentLabel(MarketplaceServiceMessage<IOrderReference> message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}GetShipmentLabelAsync");

            var result = new HttpMarketplaceMessage<ServiceMessageResult<ShipmentLabel>>(message.Identity, message.AccountConfiguration);

            if (message.Data != null)
            {
                request.SetJsonContent(message.Data);
            }

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult, GetFormatter());

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro realizar a busca da etiqueta do pedido {message.Data} no marketplace {message.Marketplace.ToString()}", $"{result?.Data}", ErrorType.Technical));
                }
            }

            return result;
            #endregion
        }

        #endregion

        #region [Monitor]

        public async Task<HttpMarketplaceMessage> ConsumeCommands(MarketplaceServiceMessage message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}ConsumeCommands");

            var result = new HttpMarketplaceMessage<ServiceMessageResult>(message.Identity, message.AccountConfiguration);

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult);

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro ao executar o consumo de comandos da monitoria", $"StatusCode: {result.StatusCode}", ErrorType.Technical));
                }
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> MonitorProductStatus(MarketplaceServiceMessage message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}MonitorProductStatus");

            var result = new HttpMarketplaceMessage<ServiceMessageResult>(message.Identity, message.AccountConfiguration);

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult);

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro ao executar a monitoria de status de produto", $"StatusCode: {result.StatusCode}", ErrorType.Technical));
                }
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<ServiceMessageResult<List<IntegrationSummary>>>> GetIntegrationSummary(MarketplaceServiceMessage message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}GetIntegrationSummary");

            var result = new HttpMarketplaceMessage<ServiceMessageResult<List<IntegrationSummary>>>(message.Identity, message.AccountConfiguration);

            request.SetHeaders(GetAuthentication(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult, GetFormatter());

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro ao obter os dados de integração", $"StatusCode: {result.StatusCode}", ErrorType.Technical));
                }
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage<ServiceMessageResult<EntityStateSearchResult>>> GetIntegrationByStatus(MarketplaceServiceMessage<EntityStateQuery> message)
        {
            #region [Code]
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.GetUri(message)}GetIntegrationSummary?{this.QueryStringBuilder(message)}");

            var result = new HttpMarketplaceMessage<ServiceMessageResult<EntityStateSearchResult>>(message.Identity, message.AccountConfiguration);

            request.SetHeaders(GetAuthenticationWithOutAccount(message));

            var requestResult = await this.HttpClient.SendAsync(request);

            if (requestResult != null)
            {
                result.MergeHttpResponseMessage(requestResult, GetFormatter());

                if (result.Data != null && result.Data.Errors != null && result.Data.Errors.Any())
                {
                    result.WithErrors(this.MapErrors(result.Data.Errors));
                }
                else if (!requestResult.IsSuccessStatusCode &&
                    (result.Data == null || result.Data.Errors == null))
                {
                    result.WithError(new Error($"Erro ao obter os dados de integração", $"StatusCode: {result.StatusCode}", ErrorType.Technical));
                }
            }

            return result;

            #endregion
        }

        #endregion

        #region [Private]
        private JsonMediaTypeFormatter GetFormatter()
        {
            #region [Code]
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
            #endregion
        }

        private Dictionary<string, string> GetAuthentication(MarketplaceServiceMessage message)
        {
            return new Dictionary<string, string>() { { "x-VendorId", message.Identity.GetVendorId() }, { "x-TenantId", message.Identity.GetTenantId() }, { "x-AccountId", message.Identity.GetAccountId() } };
        }

        private Dictionary<string, string> GetAuthenticationWithOutAccount(MarketplaceServiceMessage message)
        {
            return new Dictionary<string, string>() { { "x-VendorId", message.Identity.GetVendorId() }, { "x-TenantId", message.Identity.GetTenantId() } };
        }

        private Uri GetUri<T>(MarketplaceServiceMessage<WrapperRequest<T>> message)
        {
#if DEBUG
            return new Uri($"http://localhost:80/{message.Data.OperationId}");
#endif
            return new Uri($"http://marketplace-{Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value.ToLower()}-{message.Marketplace.ToString().ToLower()}.Yahhub.com.br/{message.Data.OperationId}");
        }

        private Uri GetUri(MarketplaceServiceMessage message)
        {
#if DEBUG
            return new Uri($"http://localhost:80/");
#endif
            return new Uri($"http://marketplace-{Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value.ToLower()}-{message.Marketplace.ToString().ToLower()}.Yahhub.com.br/");
        }

        private string QueryStringBuilder(MarketplaceServiceMessage<EntityStateQuery> message)
        {
            #region [Code]
            var query = $"statuses={message.Data.Statuses}&hasError={message.Data.HasErrors}";

            if (message.Data.Paging.Offset!= null)
            {
                query += $"&offset={message.Data.Paging.Offset}";
            }

            if (message.Data.Paging.Limit != null)
            {
                query += $"&limit={message.Data.Paging.Limit}";
            }

            return query;
            #endregion
        }

        private List<Error> MapErrors(List<ErrorResult> errors)
        {
            if(errors == null || !errors.Any())
            {
                return new List<Error>();
            }

            var result = new List<Error>();

            errors.ForEach(x => 
            {
                result.Add(new Error(x.Message,x.Reason,x.Type));
            });

            return result;
        }
        #endregion
    }
}
