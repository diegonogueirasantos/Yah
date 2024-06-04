using System;
using Yah.Hub.Common.ServiceMessage;
using System.Xml;
using Nest;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Enums;
using StackExchange.Redis;
using Yah.Hub.Common.Services;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.Extensions;

namespace Yah.Hub.Common.AbstractRepositories.ElasticSearch
{
    public abstract class AbstractElasticSearchRepository<T> : AbstractService, IElasticSearchBaseRepository<T> where T : BaseEntity
    {
        public IElasticClient Client;

        public AbstractElasticSearchRepository(ILogger<T> logger, IConfiguration configuration, IElasticClient client) : base(configuration, logger)
        {
            this.Client = client;
        }

        /// <summary>
        /// Formata o padrão da chave.
        /// </summary>
        public abstract string FormatKey(MarketplaceServiceMessage message);

        protected string GetIndex(MarketplaceServiceMessage message)
        {
            return $"Yah-hub-{this.GetEnvironment()}-{FormatKey(message)}";
        }

        protected virtual JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };


            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return settings;
        }

        public async Task<ServiceMessage<T>> SaveAsync(MarketplaceServiceMessage<T> message)
        {
            var response = new ServiceMessage<T>(message.Identity);

            try
            {
                if (string.IsNullOrWhiteSpace(message.Data.Id))
                {
                    response.WithError(new Error("Entity could not be persisted", "Missing Entity ID", ErrorType.Technical));
                    return response;
                }

                var serializedData = JsonConvert.SerializeObject(message.Data);

                var index = await this.Client.Indices.ExistsAsync(GetIndex(message));
                if (!index.Exists)
                {
                    var createIndexResult = await this.Client.Indices.CreateAsync(GetIndex(message));
                    if (!createIndexResult.IsValid)
                        throw new Exception(createIndexResult.OriginalException.Message);

                }
                var result = await this.Client.UpdateAsync<T, dynamic>(new DocumentPath<T>($"{this.FormatId(message.Identity, message.Data.Id)}"),
                    i => i.Index(GetIndex(message)).Doc(message.Data).DocAsUpsert(true).RetryOnConflict(3)
                    );

                if (result.ApiCall.Success)
                    response.WithData(message.Data);
                else
                    response.WithError(new Error(result.OriginalException));

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while save Item {message.Data}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                response.WithError(error);
            }

            return response;
        }

        public async Task<ServiceMessage<T>> DeleteAsync(MarketplaceServiceMessage<string> message)
        {
            var response = new ServiceMessage<T>(message.Identity);

            try
            {
                if (string.IsNullOrWhiteSpace(message.Data))
                {
                    response.WithError(new Error("Entity could not be persisted", "Missing Entity ID", ErrorType.Technical));
                    return response;
                }

                var serializedData = JsonConvert.SerializeObject(message.Data);

                var index = await this.Client.Indices.ExistsAsync(GetIndex(message));
                if (!index.Exists)
                {
                    var createIndexResult = await this.Client.Indices.CreateAsync(GetIndex(message));
                    createIndexResult.GetHashCode();
                }
                var result = await this.Client.DeleteAsync(new DocumentPath<T>($"{this.FormatId(message.Identity, message.Data)}"), t => t.Index(GetIndex(message)));

                if (!result.ApiCall.Success)
                    response.WithError(new Error(result.OriginalException));

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while delete Item {message.Data}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                response.WithError(error); 
            }

            return response;
        }

        public async Task<ServiceMessage.ServiceMessage> SaveBulkAsync(MarketplaceServiceMessage<List<T>> message)
        {
            var bulk = new BulkDescriptor();
            var response = ServiceMessage.ServiceMessage.CreateValidResult(message.Identity);

            await this.CreateIfNotExists(message);

            try
            {
                foreach (var item in message.Data)
                {
                    bulk.Update<T>(x => x
                        .Index(GetIndex(message))
                        .Id($"{this.FormatId(message.Identity,item.Id)}")
                        .Doc(item)
                        .RetriesOnConflict(3)
                        .DocAsUpsert(true));

                    var result = await Client.BulkAsync(bulk);

                    if (!result.ApiCall.Success)
                    {
                        return ServiceMessage.ServiceMessage.CreateInvalidResult(message.Identity, new Error("Erro ao criar item no ElasticSearch", "Erro ao criar item no ElasticSearch", ErrorType.Technical));
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while save bulk Items {message.Data}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                response.WithError(error);
            }

            return response;
        }


        public async Task<ServiceMessage<T>> GetAsync(MarketplaceServiceMessage<string> entityId)
        {
            var response = new ServiceMessage<T>(entityId.Identity);

            try
            {
                var result = await this.Client.GetAsync<T>($"{this.FormatId(entityId.Identity, entityId.Data)}", i => i.Index(GetIndex(entityId)));

                if (result.ApiCall.HttpStatusCode == 200 || result.ApiCall.HttpStatusCode == 201 || result.ApiCall.HttpStatusCode == 404)
                    response.WithData(result.Source);
                else
                    response.WithError(new Error(result.OriginalException));
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while get Item {entityId.Data}");
                Logger.LogCustomCritical(error, entityId.Identity, entityId.Data);
                response.WithError(error);
            }

            return response;
        }

        public static double GetDoubleValue(ValueAggregate item)
        {
            var result = item?.Value ?? 0;
            return Math.Round(item.Value ?? result, 2);
        }

        public static int GetIntValue(ValueAggregate item)
        {
            var result = item?.Value ?? 0;
            return Convert.ToInt32(result);
        }

        public static string GetTextValue(ValueAggregate item)
        {
            var result = string.Empty;
            if (item == null)
                return result;
            return item.Value != null ? Convert.ToString(item.Value) : result;
        }

        public async Task<ServiceMessage.ServiceMessage> CreateIfNotExists(MarketplaceServiceMessage message)
        {
            var resut = new ServiceMessage.ServiceMessage(message.Identity);
            var index = await this.Client.Indices.ExistsAsync(GetIndex(message));
            if (!index.Exists)
            {
                var createIndexResult = await this.Client.Indices.CreateAsync(GetIndex(message));
                if (!createIndexResult.IsValid)
                    resut.WithError(new Error("Erro na tentativa de criar um novo index no Elastic", "", ErrorType.Technical));
            }
            return resut;
        }

        public virtual string FormatId(Identity.Identity identity, string id)
        {
            return $"{identity.GetVendorId()}-{identity.GetTenantId()}-{identity.GetAccountId()}-{id}";
        }
    }
}