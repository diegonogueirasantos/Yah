using System;
using Nest;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Http;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Token;
using Token = Yah.Hub.Marketplace.MercadoLivre.Application.Models.Token.Token;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Errors;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using Newtonsoft.Json;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales;
using Amazon.Runtime.Internal;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime.Internal.Transform;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.Announcement;
using System.Text;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.OrderStatus;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Net.Http.Formatting;
using System.Dynamic;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Category;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.Marketplace.Interfaces;
using ErrorResult = Yah.Hub.Marketplace.MercadoLivre.Application.Models.Errors.ErrorResult;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Infractions;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Client
{
    public class MercadoLivreClient : AbstractHttpClient, IMercadoLivreClient
    {

        // TEMPORARY
        //private string baseUri = "https://api.mercadolibre.com";
        private string baseAuth = "https://auth.mercadolivre.com.br";
        private string retriveTokenUri = "oauth/token";

        // TEMPORARY APP AUTH
        private KeyValuePair<string, string> secretKey = new KeyValuePair<string, string>("client_secret", "hl5RT8TLxkT8GTvZ8qz2ts2sltXefqAm");
        private KeyValuePair<string, string> clientId = new KeyValuePair<string, string>("client_id", "5244656805001345");

        private KeyValuePair<string, string> RedirectUri(MarketplaceServiceMessage message)
        {
            return new KeyValuePair<string, string>("redirect_uri", Configuration["Marketplace:Redirect_Uri"]);
        }

        public MercadoLivreClient(HttpClient httpClient, ILogger<MercadoLivreClient> logger, IConfiguration configuration, IThrottlingService throttlingService) : base(httpClient, logger, configuration, throttlingService)
        {
        }

        #region Category

        public async Task<HttpMarketplaceMessage<List<MeliCategory>>> GetCategory(MarketplaceServiceMessage<string?> marketplaceServiceMessage)
        {
            #region Code

            var result = new HttpMarketplaceMessage<List<MeliCategory>>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);
            var uri = "/sites/MLB/categories";
            bool isSingle = default(bool);

            

            if (marketplaceServiceMessage.Data != null)
            {
                uri = $"/categories/{marketplaceServiceMessage.Data}";
                isSingle = true;
            }


            var request = new HttpRequestMessage(HttpMethod.Get, uri);


            var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };

            request.SetHeaders(headers);

            try
            {
                var requestResult = ExecuteRequest((request, EntityType.Category).AsMarketplaceServiceMessage(marketplaceServiceMessage)).GetAwaiter().GetResult();

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.Data = new List<MeliCategory>();

                    if (isSingle)
                        result.Data.Add(requestResult.Content.ReadAsAsync<MeliCategory>().GetAwaiter().GetResult());
                    else
                        result.Data.AddRange(requestResult.Content.ReadAsAsync<List<MeliCategory>>().GetAwaiter().GetResult());
                }
                else
                    result.WithErrors(this.MeliErrors(requestResult.Content.ReadAsAsync<ErrorResult>().GetAwaiter().GetResult()));

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get category");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }


        public async Task<HttpMarketplaceMessage<List<MeliCategoryAttribute>>> GetCategoryAttributes(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region Code

            var result = new HttpMarketplaceMessage<List<MeliCategoryAttribute>>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);
            var request = new HttpRequestMessage(HttpMethod.Get, $"/categories/{marketplaceServiceMessage.Data}/attributes");

            try
            {
                var requestResult = ExecuteRequest((request, EntityType.Category).AsMarketplaceServiceMessage(marketplaceServiceMessage)).GetAwaiter().GetResult();

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                else
                    result.WithErrors(this.MeliErrors(requestResult.Content.ReadAsAsync<ErrorResult>().GetAwaiter().GetResult()));

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get category attributes");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        #endregion


        #region Auth

        public async Task<HttpMarketplaceMessage<Token>> GetAccessToken(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<Token>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {

                // TODO: add headers ??
                //request.SetHeaders(new Dictionary<string, string> { { "TOKEN", "VALUE" } });

                // set content
                var kvp = new List<KeyValuePair<string, string>>();
                kvp.Add(new KeyValuePair<string, string>("code", marketplaceServiceMessage.Data));
                kvp.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                kvp.Add(new KeyValuePair<string, string>("code_verifier", $"{marketplaceServiceMessage.Identity.GetVendorId()}-{marketplaceServiceMessage.Identity.GetTenantId()}-{marketplaceServiceMessage.Identity.GetAccountId()}"));
                kvp.Add(RedirectUri(marketplaceServiceMessage));
                var content = GetAuthGrant(kvp);

                request.SetEncodedContent(content);

                // set method
                request.Method = HttpMethod.Post;

                request.RequestUri = new Uri($"{retriveTokenUri}", UriKind.Relative);

                var requestResult = ExecuteRequest((request,EntityType.Configuration).AsMarketplaceServiceMessage(marketplaceServiceMessage)).GetAwaiter().GetResult();

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get access token");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<Token>> RefreshToken(MarketplaceServiceMessage marketplaceServiceMessage)
        {
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<Token>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {

                // TODO: add headers ??
                //request.SetHeaders(new Dictionary<string, string> { { "TOKEN", "VALUE" } });

                // set content
                var kvp = new List<KeyValuePair<string, string>>();
                kvp.Add(new KeyValuePair<string, string>("refresh_token", marketplaceServiceMessage.AccountConfiguration.RefreshToken));
                kvp.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                var content = GetAuthGrant(kvp);

                request.SetEncodedContent(content);

                // set method
                request.Method = HttpMethod.Post;

                request.RequestUri = new Uri($"{retriveTokenUri}", UriKind.Relative);

                var requestResult = await ExecuteRequest((request, EntityType.Configuration).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }
                
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while refresh token");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        private Dictionary<string, string> GetAuthGrant(List<KeyValuePair<string, string>> keyValuePair)
        {
            var content = new Dictionary<string, string>();

            content.Add(secretKey.Key, secretKey.Value);
            content.Add(clientId.Key, clientId.Value);

            keyValuePair.ForEach(kvp =>
            {
                content.Add(kvp.Key, kvp.Value);
            });


            return content;
        }

        public async Task<MarketplaceServiceMessage<string>> GetAuthorizationUrl(MarketplaceServiceMessage message)
        {
            var result = new MarketplaceServiceMessage<string>(message.Identity, message.AccountConfiguration);
            result.WithData($"{baseAuth}/authorization/?response_type=code&{this.clientId.Key}={clientId.Value}&redirect_uri={Configuration["Marketplace:Redirect_Uri"]}&state={message.Identity.GetVendorId()}-{message.Identity.GetTenantId()}-{message.Identity.GetAccountId()}");
            return result;
        }

        #endregion

        #region Annoucemente Create / Update / Delete

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> GetMeliAnnouncement(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"/items/{marketplaceServiceMessage.Data}", UriKind.Relative);
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get announcement");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<MercadoLivreInfractions>> GetMeliInfractionsByAnnouncementId(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<MercadoLivreInfractions>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"/moderations/infractions/{marketplaceServiceMessage.AccountConfiguration.AppId}?related_item_id={marketplaceServiceMessage.Data}", UriKind.Relative);
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get infractions from announcement");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> CreateAnnoucement(MarketplaceServiceMessage<MeliAnnouncement> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = "items";

                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url, UriKind.Relative));
                request.SetJsonContent(marketplaceServiceMessage.Data, GetFormatterForInsert());
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                    result.StatusCode = requestResult.StatusCode;
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while create announcement");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> UpdateAnnoucement(MarketplaceServiceMessage<MeliAnnouncement> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            marketplaceServiceMessage.Data.Description = null;
            var meliId = marketplaceServiceMessage.Data.ItemId;
            marketplaceServiceMessage.Data.ItemId = null;

            try
            {
                var url = $"items/{meliId}";

                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };

                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                request.SetJsonContent(marketplaceServiceMessage.Data,GetFormatter());
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                    result.StatusCode = requestResult.StatusCode;
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while update announcement");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> ChangeListingItemState(MarketplaceServiceMessage<(AnnouncementStatus status, string ItemId)> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}";

                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };

                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                request.SetJsonContent(marketplaceServiceMessage.Data.status, GetFormatter());
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while update announcement");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> UpdateAnnoucementInventory(MarketplaceServiceMessage<MeliAnnoucementInventory> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.SetJsonContent(marketplaceServiceMessage.Data, GetFormatter());
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while update announcement inventory");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> UpdateAnnoucementPrice(MarketplaceServiceMessage<MeliAnnoucementPrice> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.SetJsonContent(marketplaceServiceMessage.Data, GetFormatter());
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while update announcement price");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        #region Description Create / Update
        public async Task<HttpMarketplaceMessage> CreateAnnoucementDescription(MarketplaceServiceMessage<MeliAnnouncement> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try 
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}/description";
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url, UriKind.Relative));
                request.SetJsonContent(new { plain_text = marketplaceServiceMessage.Data.Description.PlainText });
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult);

                if (requestResult != null && !requestResult.IsSuccessStatusCode)
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while create announcement description");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage> UpdateAnnoucementDescription(MarketplaceServiceMessage<MeliAnnouncement> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}/description";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };

                request.SetJsonContent(new { plain_text = marketplaceServiceMessage.Data.Description.PlainText });
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult);

                if (requestResult != null && !requestResult.IsSuccessStatusCode)
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while update announcement description");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> SetAnnoucementStatus(MarketplaceServiceMessage<UpdateAnnoucementStatus> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.SetJsonContent(new { status = marketplaceServiceMessage.Data.Status.ToString() });
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while update announcement status");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<HttpMarketplaceMessage<MeliAnnouncement>> DeleteAnnoucement(MarketplaceServiceMessage<UpdateAnnoucementStatus> marketplaceServiceMessage)
        {
            var result = new HttpMarketplaceMessage<MeliAnnouncement>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var url = $"items/{marketplaceServiceMessage.Data.ItemId}";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url, UriKind.Relative));
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.SetJsonContent(new { deleted = true });
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Announcement).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.TryReadAsAsync<ErrorResult>(marketplaceServiceMessage);
                    result.WithErrors(this.MeliErrors(errorResult.Data));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while delete announcement");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        #endregion

        #region Order

        public async Task<HttpMarketplaceMessage<OrderClientResult>> GetOrdersForIntegration(MarketplaceServiceMessage<OrderQueryRequest> marketplaceServiceMessage)
        {
            #region Code
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<OrderClientResult>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            #region QueryString
            var queryString = new Dictionary<string, string>();
            queryString.Add("seller", marketplaceServiceMessage.AccountConfiguration.RefreshToken.Split('-').LastOrDefault());
            queryString.Add("sort", marketplaceServiceMessage.Data.Sort);
            queryString.Add("offset", marketplaceServiceMessage.Data.Offset.ToString());
            queryString.Add("limit", marketplaceServiceMessage.Data.Limit.ToString());
            queryString.Add(marketplaceServiceMessage.Data.Status.Keys.FirstOrDefault(), marketplaceServiceMessage.Data.Status.Values.FirstOrDefault());
            queryString.Add("order.date_created.from", marketplaceServiceMessage.Data.From.ToISODate());
            queryString.Add("order.date_created.to", marketplaceServiceMessage.Data.To.ToISODate());
            #endregion

            var qs = queryString.ToCreateQueryString();

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"orders/search{qs.ToUriComponent()}", UriKind.Relative);
                request.SetHeaders(headers);


                var response = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (!response.IsSuccessStatusCode)
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                    return result;
                }
                else
                {
                    result.MergeHttpResponseMessage(response, GetFormatter());
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get orders");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<HttpMarketplaceMessage<byte[]>> GetShipmentLabel(MarketplaceServiceMessage<string> shippingOrderId)
        {
            var result = new HttpMarketplaceMessage<byte[]>(shippingOrderId.Identity, shippingOrderId.AccountConfiguration);

            var queryString = new Dictionary<string, string>();
            queryString.Add("shipment_ids", string.Join(',', shippingOrderId.Data));
            queryString.Add("response_type", "pdf");

            var qs = queryString.ToCreateQueryString();

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"/shipment_labels{qs.ToUriComponent()}"));

            request.SetHeaders(new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {shippingOrderId.AccountConfiguration.AccessToken}" }
                });

            var response = await this.ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(shippingOrderId));

            if (response.IsSuccessStatusCode)
            {
                result.MergeHttpResponseMessage(response);
            }
            else
            {
                // TODO REVIEW ERROR CODE
                var content = await response.Content.ReadAsAsync<string>();
                result.WithError(new Error($"Erro ao recuperar etiqueta de envio para o pedido {shippingOrderId.Data}", content, ErrorType.Technical));
            }
           
            return result;
        }

        public async Task<HttpMarketplaceMessage<MeliOrder>>GetOrder(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]

            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<MeliOrder>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };

                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"orders/{marketplaceServiceMessage.Data}", UriKind.Relative);
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));
                result.StatusCode = requestResult.StatusCode;
                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult,GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

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

        public async Task<HttpMarketplaceMessage<Models.Sales.Shipping>> GetMeliShipping(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<Models.Sales.Shipping>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"/shipments/{marketplaceServiceMessage.Data}", UriKind.Relative);
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get shipping");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage<BillingInfoRequest>> GetBillingInfo(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage<BillingInfoRequest>(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"/orders/{marketplaceServiceMessage.Data}/billing_info", UriKind.Relative);
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                if (requestResult != null && requestResult.IsSuccessStatusCode)
                {
                    result.MergeHttpResponseMessage(requestResult, GetFormatter());
                }
                else
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get billing info");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> DeliveryOrder(MarketplaceServiceMessage<string> marketplaceServiceMessage)
        {
            #region [Code]

            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"/orders/{marketplaceServiceMessage.Data}/feedback", UriKind.Relative);
                request.Content = JsonContent.Create(new { fulfilled = true });
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult);

                if (!requestResult.IsSuccessStatusCode)
                { 
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get delivery info");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }
        

        public async Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<(string shippingId, string invoiceXML)> marketplaceServiceMessage)
        {
            #region [Code]

            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var queryString = new Dictionary<string, string>();
                queryString.Add("siteId", "MLB");
                var qs = queryString.ToCreateQueryString();
                var httpContent = new StringContent(marketplaceServiceMessage.Data.invoiceXML, Encoding.UTF8, "application/xml");
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"/shipments/{marketplaceServiceMessage.Data.shippingId}/invoice_data{qs.ToUriComponent()}", UriKind.Relative);
                request.Content = httpContent;
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));
                result.MergeHttpResponseMessage(requestResult);

                if (!requestResult.IsSuccessStatusCode)
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while invoice order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<(string shippingId, MeliInvoice invoice)> marketplaceServiceMessage)
        {
            #region [Code]

            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var queryString = new Dictionary<string, string>();
                queryString.Add("siteId", "MLB");
                var qs = queryString.ToCreateQueryString();
                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"/shipments/{marketplaceServiceMessage.Data.shippingId}/invoice_data{qs.ToUriComponent()}", UriKind.Relative);
                request.Content = JsonContent.Create(marketplaceServiceMessage.Data.invoice);
                request.SetHeaders(headers);


                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));

                result.MergeHttpResponseMessage(requestResult);

                if (!requestResult.IsSuccessStatusCode)
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while invoice order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public async Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<(string packId, byte[] xml, string fileName)> marketplaceServiceMessage)
        {
            #region [Code]

            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                form.Add(new ByteArrayContent(marketplaceServiceMessage.Data.xml, 0, marketplaceServiceMessage.Data.xml.Length), "fiscal_document", marketplaceServiceMessage.Data.fileName);

                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"/packs/{marketplaceServiceMessage.Data.packId}/fiscal_documents", UriKind.Relative);
                request.Content = form;
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));
                result.MergeHttpResponseMessage(requestResult);

                if (!requestResult.IsSuccessStatusCode)
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while invoice order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }

        public async Task<HttpMarketplaceMessage> ShippedOrder(MarketplaceServiceMessage<(string shippingId, string trackingNumber, int serviceId, long? buyer)> marketplaceServiceMessage)
        {
            #region [Code]
            var request = new HttpRequestMessage();
            var result = new HttpMarketplaceMessage(marketplaceServiceMessage.Identity, marketplaceServiceMessage.AccountConfiguration);

            try
            {
                var queryString = new Dictionary<string, string>();
                queryString.Add("caller.id", marketplaceServiceMessage.AccountConfiguration.AccountId);
                var qs = queryString.ToCreateQueryString();

                var headers = new Dictionary<string, string>
                {
                    { "x-format-new", "true" },
                    { "Authorization", $"Bearer {marketplaceServiceMessage.AccountConfiguration.AccessToken}" }
                };
                request.Method = HttpMethod.Put;
                request.RequestUri = new Uri($"/shipments/{marketplaceServiceMessage.Data.shippingId}{qs.ToUriComponent()}", UriKind.Relative);
                request.Content = JsonContent.Create(new { tracking_number = marketplaceServiceMessage.Data.trackingNumber, service_id = marketplaceServiceMessage.Data.serviceId, status = "shipped", receiver_id = marketplaceServiceMessage.Data.buyer });
                request.SetHeaders(headers);

                var requestResult = await ExecuteRequest((request, EntityType.Order).AsMarketplaceServiceMessage(marketplaceServiceMessage));
                result.MergeHttpResponseMessage(requestResult);

                if (!requestResult.IsSuccessStatusCode)
                {
                    var errorResult = await requestResult.Content.ReadFromJsonAsync<ErrorResult>();
                    result.WithErrors(this.MeliErrors(errorResult));
                }

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while shipping order");
                Logger.LogCustomCritical(error, marketplaceServiceMessage.Identity);
                result.WithError(error);
            }

            return result;
            #endregion
        }


        #endregion

        #region Private

        private List<Error> MeliErrors(ErrorResult error)
        {
            List<Error> mappedErrors = new List<Error>();

            if (error.Reasons != null && error.Reasons.Any())
            {
                error.Reasons.ForEach(reason =>
                {
                    mappedErrors.Add(new Error($"[{reason.type}] {reason.Message}", reason.ReasonCode ?? String.Empty,ErrorType.Business));
                });
            }
            else
            {
                mappedErrors.Add(new Error(error.Message, error.ErrorCode ?? String.Empty, ErrorType.Business));
            }
            return mappedErrors;
        }

        private JsonMediaTypeFormatter GetFormatterForInsert()
        {
            var formatter = new JsonMediaTypeFormatter()
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }
            };
            formatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return formatter;
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

        #endregion
    }
}

