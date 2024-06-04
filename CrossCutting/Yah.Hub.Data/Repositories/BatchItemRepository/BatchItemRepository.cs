using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Nest;
using Newtonsoft.Json;
using Yah.Hub.Common.AbstractRepositories.DynamoDB;
using Yah.Hub.Domain.BatchItem;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using System;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Data.Repositories.BatchItemRepository
{
    public class BatchItemRepository : AbstractDynamoRepository<BatchItem>, IBatchItemRepository
    {
        public BatchItemRepository(AmazonDynamoDBClient client, IConfiguration configuration, ILogger<BatchItemRepository> logger) : base(client, configuration, logger)
        {
        }

        public override Dictionary<string, AttributeValue> GetEntityKeys(ServiceMessage<BatchItem> serviceMessage)
        {
            var attributes = new Dictionary<string, AttributeValue>()
            {
                { "entityId", new AttributeValue { S = serviceMessage.Data.EntityId ?? string.Empty }},
                { "status", new AttributeValue { S = serviceMessage.Data.Status.ToString() }},
                { "data", new AttributeValue { S = serviceMessage.Data.Data.CompressToGzip() ?? string.Empty }},
                { "timestamp", new AttributeValue { S = serviceMessage.Data.Timestamp.ToISODate() }},
                { "type", new AttributeValue { S = serviceMessage.Data.Type.ToString() }},
                { "commandType", new AttributeValue { S = serviceMessage.Data.CommandType.ToString() }},
                { "TTL", new AttributeValue { N = Convert.ToString(serviceMessage.Data.TTL.ToUnixTimeSeconds())}},
                { "marketplace", new AttributeValue { N = serviceMessage.Data.Marketplace.ToString()}}
            };

            return attributes;
        }

        public override string GetPartitionKey()
        {
            return "vendorId-tenantId-accountId-type-entityId";
        }

        public override AttributeValue GetPartitionKeyValue(ServiceMessage<BatchItem> serviceMessage)
        {
            return new AttributeValue($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}-{serviceMessage.Data.Type}-{serviceMessage.Data.EntityId}");
        }

        public override BatchItem GetResultValue(GetItemResponse result)
        {
            return new BatchItem()
            {
                EntityId = result.Item["entityId"].S,
                Status = result.Item["status"].S.ToEnum<BatchStatus>(),
                Data = result.Item["data"].S.DecompressFromGzip(),
                Timestamp = DateTimeOffset.Parse(result.Item["data"].S),
                Type = result.Item["type"].S.ToEnum<BatchType>(),
                CommandType = result.Item["commandType"].S.ToEnum<Operation>(),
                TTL = DateTimeOffset.Parse(result.Item["TTL"].S),
                Marketplace = result.Item["marketplace"].S.ToEnum<MarketplaceAlias>()
            };
        }

        public override string GetSecundaryKey()
        {
            return "vendorId-tenantId-accountId-entityId";
        }

        public override AttributeValue GetSecundaryKeyValue(ServiceMessage<BatchItem> serviceMessage)
        {
            return new AttributeValue($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}-{serviceMessage.Data.EntityId}");
        }

        public override string GetTableName()
        {
            return "batch_items";
        }

        protected override AttributeValue GetSortKeyValue(ServiceMessage<BatchItem> entity)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceMessage<List<BatchItem>>> QueryAsync(ServiceMessage<BatchItemQuery> serviceMessage)
        {
            #region [Code]

            var vendor = serviceMessage.Identity.GetVendorId();
            var tenant = serviceMessage.Identity.GetTenantId();
            var account = serviceMessage.Identity.GetAccountId();
            var batch = serviceMessage.Data;

            var result = ServiceMessage<List<BatchItem>>.CreateValidResult(serviceMessage.Identity);

            Dictionary<string, AttributeValue> lastKeyEvaluated = null;

            var valueRequest = $"{vendor}-{tenant}-{account}-{batch.Status}-{batch.CommandType}";

            try
            {

                do
                {
                    var queryRequest = new QueryRequest()
                    {
                        TableName = GetTable(),
                        IndexName = "vendorId-tenantId-accountId-status-commandType-index",
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            { "#qr1", "qr1" }
                        },
                        KeyConditionExpression = $"#qr1 = :qr1",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            { $":qr01", new AttributeValue { S = $"{valueRequest}" } }
                        },
                        Limit = 10,
                        ExclusiveStartKey = lastKeyEvaluated,
                        Select = Select.ALL_PROJECTED_ATTRIBUTES
                    };

                    var response = await base.GetItemByQuery(queryRequest);

                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {response.HttpStatusCode} | ConsumedCapacity: {response.ConsumedCapacity} | Metadata: {response.ResponseMetadata}", ErrorType.Technical));

                    foreach (Dictionary<string, AttributeValue> item in response.Items)
                    {
                        var batchItem = GetResultValue(item);

                        switch (batchItem.Type)
                        {
                            case BatchType.PRODUCT:
                                batchItem.Product = JsonConvert.DeserializeObject<Product>(batchItem.Data);
                                break;

                            case BatchType.PRICE:
                                batchItem.Price = JsonConvert.DeserializeObject<ProductPrice>(batchItem.Data);
                                break;

                            case BatchType.INVENTORY:
                                batchItem.Inventory = JsonConvert.DeserializeObject<ProductInventory>(batchItem.Data);
                                break;
                        }
                        result.Data.Add(batchItem);
                    }

                    lastKeyEvaluated = response.LastEvaluatedKey;
                }
                while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;

            #endregion
        }
    }
}
