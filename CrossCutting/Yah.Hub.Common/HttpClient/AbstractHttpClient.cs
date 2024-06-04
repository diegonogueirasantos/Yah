using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Yah.Hub.Common.Http
{
    public abstract class AbstractHttpClient
    {
        protected HttpClient HttpClient { get; set; }
        protected ILogger Logger { get; private set; }
        protected IConfiguration Configuration { get; private set; }
        protected readonly IThrottlingService ThrottlingService;

        public AbstractHttpClient(HttpClient httpClient, ILogger logger, IConfiguration configuration, IThrottlingService throttling)
        {
            this.HttpClient = httpClient;
            this.Logger = logger;
            this.Configuration = configuration;
            this.ThrottlingService = throttling;
        }

        /// <summary>
        /// Execute request
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> ExecuteRequest(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            if (this.UseThrottlingControl() && marketplaceServiceMessage.Data.entityType != EntityType.Configuration)
            {
                if(await this.ThrottlingService.ConsumeThrottling(marketplaceServiceMessage, FormatThrottlingKey(marketplaceServiceMessage), () => GetRateLimit(marketplaceServiceMessage)))
                {
                    var result =  new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests);
                    result.Headers.Add("type", "internal");
                    return result;
                }
            }

            marketplaceServiceMessage.Data.requestMessage.RequestUri = new System.Uri($"{GetBaseUri()}/{marketplaceServiceMessage.Data.requestMessage.RequestUri.ToString()}");
            return await this.HttpClient.SendAsync(marketplaceServiceMessage.Data.requestMessage);
        }

        /// <summary>
        /// Execute request
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> ExecuteCustomRequest(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage)
        {
            if (this.UseThrottlingControl() && marketplaceServiceMessage.Data.entityType != EntityType.Configuration)
            {
                if (!await this.ThrottlingService.ConsumeThrottling(marketplaceServiceMessage, FormatThrottlingKey(marketplaceServiceMessage), () => GetRateLimit(marketplaceServiceMessage)))
                {
                    var result = new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests);
                    result.Headers.Add("type", "internal");
                    return result;
                }
            }

            marketplaceServiceMessage.Data.requestMessage.RequestUri = marketplaceServiceMessage.Data.requestMessage.RequestUri;
            return await this.HttpClient.SendAsync(marketplaceServiceMessage.Data.requestMessage);
        }

        /// <summary>
        /// Get Base Uri from marketplace configuration
        /// </summary>
        /// <returns></returns>
        public virtual string GetBaseUri()
        {
            return Configuration["Marketplace:BaseUri"];
        }

        #region Abstract

        public abstract bool UseThrottlingControl();
        public abstract string FormatThrottlingKey(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage);
        public abstract int? GetRateLimit(MarketplaceServiceMessage<(HttpRequestMessage requestMessage, EntityType entityType)> marketplaceServiceMessage);

        #endregion


    }
}

