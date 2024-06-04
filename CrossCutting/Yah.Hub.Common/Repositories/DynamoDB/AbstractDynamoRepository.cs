using System;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Nest;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Services;

namespace Yah.Hub.Common.AbstractRepositories.DynamoDB
{
    public abstract class AbstractDynamoRepository<T> : AbstractService, IDynamoRepository<T>
    {
        private readonly AmazonDynamoDBClient Client;
        private readonly IConfiguration Configuration;

        public abstract string GetTableName();
        public abstract string GetPartitionKey();
        public abstract string GetSecundaryKey();


        public AbstractDynamoRepository(AmazonDynamoDBClient client, IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
            this.Client = client;
            this.Configuration = configuration;
        }

        public abstract Dictionary<string, AttributeValue> GetEntityKeys(ServiceMessage<T> serviceMessage);
        public abstract AttributeValue GetPartitionKeyValue(ServiceMessage<T> serviceMessage);
        public abstract AttributeValue GetSecundaryKeyValue(ServiceMessage<T> serviceMessage);
        protected abstract AttributeValue GetSortKeyValue(ServiceMessage<T> entity);
        public abstract T GetResultValue(GetItemResponse result);
        public T GetResultValue(Dictionary<string, AttributeValue> result) { return GetResultValue(new GetItemResponse() { Item = result }); }


        protected string GetTable()
        {
            return $"Yah_hub_{this.GetEnvironment()}_{GetTableName()}";
        }

        private void SetIdentity(ServiceMessage<T> serviceMessage, Dictionary<string, AttributeValue> keyValuePairs)
        {
            var identity = new Dictionary<string, AttributeValue>()
               {
                   { "vendorId", new AttributeValue { S = serviceMessage.Identity.GetVendorId() }},
                   { "tenantId", new AttributeValue { S = serviceMessage.Identity.GetTenantId() }},
                   { "accountId", new AttributeValue { S = serviceMessage.Identity.GetAccountId() }},
               };

            foreach (var item in identity)
                keyValuePairs.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Add or Update Item
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public async Task<ServiceMessage.ServiceMessage> UpsertItem(ServiceMessage<T> serviceMessage)
        {
            var result = ServiceMessage.ServiceMessage.CreateValidResult(serviceMessage.Identity);

            try
            {
                // set entity keys
                var entityKeys = GetEntityKeys(serviceMessage);

                SetIdentity(serviceMessage, entityKeys);

                // primary key
                entityKeys.Add(GetPartitionKey(), GetPartitionKeyValue(serviceMessage));

                // secundary key
                //entityKeys.Add(GetSecundaryKey(), GetSecundaryKeyValue(serviceMessage));

                // create put item
                var putItem = new PutItemRequest(GetTable(), entityKeys);
                var putResult = await Client.PutItemAsync(putItem);

                // review this
                if (putResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.WithError(new Error("Error on UPSERT entity on DynamoDB", $"HTTP Status Code: {putResult.HttpStatusCode} | ConsumedCapacity: {putResult.ConsumedCapacity} | Metadata: {putResult.ResponseMetadata}", ErrorType.Technical));
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        /// <summary>
        /// Return item by its partition keys
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public async Task<ServiceMessage<T>> GetItemByPartitionKey(ServiceMessage<string> serviceMessage)
        {
            var result = ServiceMessage.ServiceMessage<T>.CreateValidResult(serviceMessage.Identity);

            try
            {
                var getRequest = new GetItemRequest(GetTable(), new Dictionary<string, AttributeValue>() { { GetPartitionKey(), new AttributeValue(serviceMessage.Data) } });
                var getResult = await Client.GetItemAsync(getRequest);

                if (getResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {getResult.HttpStatusCode} | ConsumedCapacity: {getResult.ConsumedCapacity} | Metadata: {getResult.ResponseMetadata}", ErrorType.Technical));

                result.WithData(GetResultValue(getResult));
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        /// <summary>
        /// Return item by its partition keys
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public async Task<ServiceMessage<List<T>>> GetItemBySecundaryKey(ServiceMessage<string> serviceMessage)
        {
            var result = ServiceMessage.ServiceMessage<List<T>>.CreateValidResult(serviceMessage.Identity);
            result.Data = new List<T>();
            try
            {
                var getRequest = new BatchGetItemRequest(new Dictionary<string, KeysAndAttributes>()
                {
                    { GetTableName(),
                        new KeysAndAttributes
                            {
                                Keys = new List<Dictionary<string, AttributeValue>>()
                                    {
                                        new Dictionary<string, AttributeValue>() { { GetPartitionKey(), new AttributeValue(serviceMessage.Data) }
                                    }
                                }
                            }
                        }
                     }
                );


                var getResult = await Client.BatchGetItemAsync(getRequest);

                if (getResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {getResult.HttpStatusCode} | ConsumedCapacity: {getResult.ConsumedCapacity} | Metadata: {getResult.ResponseMetadata}", ErrorType.Technical));

                foreach (var item in getResult.Responses[GetTable()])
                    result.Data.Add(GetResultValue(item));
                
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        public async Task<QueryResponse> GetItemByQuery(QueryRequest query)
        {
            if(query == null)
            {
                throw new ArgumentNullException("query");
            }

            var response = await Client.QueryAsync(query);

            return response;
        }

        public async Task<ScanResponse> GetItemByScan(ScanRequest scan)
        {
            if (scan == null)
            {
                throw new ArgumentNullException("scan");
            }

            var response = await Client.ScanAsync(scan);

            return response;
        }


    }
}