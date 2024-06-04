using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Elasticsearch.Net.Specification.IndicesApi;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Common.ServiceMessage
{
    public class HttpMarketplaceMessage: MarketplaceServiceMessage
    {
        public HttpMarketplaceMessage(Yah.Hub.Common.Identity.Identity identity, AccountConfiguration account) : base(identity, account)
        {
        }

        public virtual HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Read content and desserialize, set Http status to marketplaceMessage
        /// </summary>
        /// <param name="response"></param>
        public virtual void MergeHttpResponseMessage(HttpResponseMessage response)
        {
            try
            {
                // status code
                this.StatusCode = response.StatusCode;  
            }
            catch (Exception ex)
            {
                this.Errors.Add(new Error(ex));
            }
        }
    }

    [Obsolete("Use HttpMarketplaceMessage<SuccessResponse, ErrorResponse> instead")]
    public class HttpMarketplaceMessage<SuccessResponse> : HttpMarketplaceMessage
    {
        public HttpMarketplaceMessage(Yah.Hub.Common.Identity.Identity identity, AccountConfiguration account) : base(identity, account)
        {
        }

        public SuccessResponse Data { get; set; }

        /// <summary>
        /// Set Http status to marketplaceMessage
        /// </summary>
        /// <param name="response"></param>
        public void MergeHttpResponseMessage(HttpResponseMessage response, params MediaTypeFormatter[]? mediaTypeFormatter)
        {
            try
            {
                base.MergeHttpResponseMessage(response);
                
                if (response.IsSuccessStatusCode && response.Content != null)
                    Data = response.Content.ReadAsAsync<SuccessResponse>(mediaTypeFormatter ?? null).GetAwaiter().GetResult();
                else
                    this.Errors.Add(new Error("Error while execute request", $"Path: {response.RequestMessage.RequestUri}, Method: {response.RequestMessage.Method}", ErrorType.Technical));
            }
            catch (Exception ex)
            {
                this.Errors.Add(new Error(ex));
            }
        }
    }

    public class HttpMarketplaceMessage<SuccessResponse, ErrorResponse> : HttpMarketplaceMessage<SuccessResponse>
    {
        public HttpMarketplaceMessage(Yah.Hub.Common.Identity.Identity identity, AccountConfiguration account) : base(identity, account)
        {
        }

        public SuccessResponse Data { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Set Http status to marketplaceMessage, with content and error handle (recomended)
        /// </summary>
        /// <param name="response"></param>
        public void MergeHttpResponseMessage(HttpResponseMessage response, Func<ErrorResponse, List<Error>> mapFunc, params MediaTypeFormatter[]? mediaTypeFormatter)
        {
            try
            {
                base.MergeHttpResponseMessage(response, mediaTypeFormatter);

                var Errors = new List<Error>();

                if (!response.IsSuccessStatusCode && response.Content != null)
                     Errors = mapFunc(response.Content.ReadAsAsync<ErrorResponse>(mediaTypeFormatter ?? null).GetAwaiter().GetResult());

                if (Errors != null && Errors.Any())
                    this.Errors = Errors;
                else if(!this.Errors.Any())
                    this.Errors.Add(new Error("Error while execute request", $"Path: {response.RequestMessage.RequestUri}, Method: {response.RequestMessage.Method}", ErrorType.Technical));
            }
            catch (Exception ex)
            {
                this.Errors.Add(new Error(ex));
            }
        }

        /// <summary>
        /// Set Http status to marketplaceMessage, with content and error handle (recomended)
        /// </summary>
        /// <param name="response"></param>
        public void MergeHttpResponseMessage(HttpResponseMessage response, Func<HttpResponseMessage, List<Error>> extractFunc, params MediaTypeFormatter[]? mediaTypeFormatter)
        {
            try
            {
                base.MergeHttpResponseMessage(response, mediaTypeFormatter);

                var Errors = new List<Error>();

                if (!response.IsSuccessStatusCode)
                    Errors = extractFunc(response);

                if (Errors != null && Errors.Any())
                    this.Errors = Errors;
            }
            catch (Exception ex)
            {
                this.Errors.Add(new Error(ex));
            }
        }
    }
}
