using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Common.AbstractRepositories.DynamoDB;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Announcement;
using System;
using Yah.Hub.Domain.Order;
using Yah.Hub.Common.Security;
using Yah.Hub.Domain.Catalog;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Xml.Linq;

namespace Yah.Hub.Marketplace.Application.Repositories.Announcement
{
    public class AnnouncementRepository : AbstractDynamoRepository<Domain.Announcement.Announcement>, IAnnouncementRepository
    {
        private readonly IConfiguration Configuration;
        public AnnouncementRepository(AmazonDynamoDBClient client, IConfiguration configuration, ILogger<AnnouncementRepository> logger) : base(client,configuration, logger)
        {
            this.Configuration = configuration;

        }

        public override Dictionary<string, AttributeValue> GetEntityKeys(ServiceMessage<Domain.Announcement.Announcement> serviceMessage)
        {

            var itemId = !String.IsNullOrEmpty(serviceMessage.Data.MarketplaceId) ? serviceMessage.Data.MarketplaceId : "0";

            var attribute = new Dictionary<string, AttributeValue>()
               {
                   { "sortKey", GetSortKeyValue(serviceMessage)},
                   { "id", new AttributeValue { S = serviceMessage.Data.Id }},
                   { "isActive", new AttributeValue { BOOL = serviceMessage.Data.IsActive }},
                   { "isDeleted", new AttributeValue { BOOL = serviceMessage.Data.IsDeleted }},
                   { "productId", new AttributeValue { S = serviceMessage.Data.ProductId }},
                   { "timestamp", new AttributeValue(serviceMessage.Data.Timestamp.ToISODate())},
                   { "item", new AttributeValue { S = Newtonsoft.Json.JsonConvert.SerializeObject(serviceMessage.Data.Item) }},
                   { "product", new AttributeValue { S = Newtonsoft.Json.JsonConvert.SerializeObject(serviceMessage.Data.Product) }},
                   { "vendorId-tenantId-productId", new AttributeValue { S = $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Data.ProductId}" } },
                   { "vendorId-tenantId-accountId", new AttributeValue { S = $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}" } },
                   { "vendorId-tenantId-id", new AttributeValue { S = $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Data.Id}" } },
                   { "marketplaceId", new AttributeValue { S = itemId } }
               };


            return attribute;
        }

        public override string GetPartitionKey()
        {
            return $"vendorId-tenantId";
        }

        public override AttributeValue GetPartitionKeyValue(ServiceMessage<Domain.Announcement.Announcement> serviceMessage)
        {
            return new AttributeValue($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}");
        }

        public override Domain.Announcement.Announcement GetResultValue(GetItemResponse result)
        {
            return new Domain.Announcement.Announcement(result.Item["vendorId"].S, result.Item["tenantId"].S, result.Item["accountId"].S, result.Item["id"].S)
            {
                MarketplaceId = result.Item["marketplaceId"].S,
                IsActive = result.Item["isActive"].BOOL,
                IsDeleted = result.Item.GetValueOrDefault("isDeleted")?.BOOL ?? false,
                ProductId = result.Item["productId"].S,
                Item = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnouncementItem>(result.Item["item"].S),
                Product = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(result.Item["product"].S),
                Timestamp = Convert.ToDateTime(result.Item["timestamp"].S)
            };
        }

        public override string GetTableName()
        {
            return $"announcement";
        }

        public string MarketplaceId()
        {
            return $"vendorId-tenantId-accountId-marketplaceId";
        }

        public override string GetSecundaryKey()
        {
            return $"vendorId-tenantId-productId";
        }

        public override AttributeValue GetSecundaryKeyValue(ServiceMessage<Domain.Announcement.Announcement> serviceMessage)
        {
            return new AttributeValue($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Data.Product.Id}");
        }

        protected override AttributeValue GetSortKeyValue(ServiceMessage<Domain.Announcement.Announcement> message)
        {
            return new AttributeValue($"{message.Data.Id}");
        }

        public async Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetItemByProductId(ServiceMessage<string> serviceMessage)
        {
            #region [Code]
            var result = ServiceMessage<List<Domain.Announcement.Announcement>>.CreateValidResult(serviceMessage.Identity);
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;
            result.Data = new List<Domain.Announcement.Announcement>();

            var valueRequest = $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Data}";
            try
            {

                do
                {
                    var queryRequest = new QueryRequest()
                    {
                        TableName = GetTable(),
                        IndexName = "vendorId-tenantId-productId-index",
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            { "#vendorIdTenantIdProductId", "vendorId-tenantId-productId" }
                        },
                        KeyConditionExpression = $"#vendorIdTenantIdProductId = :vendorIdTenantIdProductId",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            { $":vendorIdTenantIdProductId", new AttributeValue { S = $"{valueRequest}" } }
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
                        result.Data.Add(GetResultValue(item));
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

        public async Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetItemByMarketplaceId(ServiceMessage<string> serviceMessage)
        {
            #region [Code]
            var result = new ServiceMessage<List<Domain.Announcement.Announcement>>(serviceMessage.Identity);
            result.Data = new List<Domain.Announcement.Announcement>();
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;

            try
            {
                do
                {
                    var queryRequest = new ScanRequest()
                    {
                        TableName = GetTable(),
                        ExclusiveStartKey = lastKeyEvaluated,
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            { "#n0", "marketplaceId" }
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":v0", new AttributeValue { S = serviceMessage.Data }}
                        },
                        FilterExpression = "#n0 = :v0",
                        Select = Select.ALL_ATTRIBUTES
                    };

                    var response = await base.GetItemByScan(queryRequest);

                    foreach (Dictionary<string, AttributeValue> item in response.Items)
                    {
                        result.Data.Add(GetResultValue(item));
                    }

                    lastKeyEvaluated = response.LastEvaluatedKey;
                }
                while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);
            }
            catch (Exception ex)
            {
                //result.WithError(new Error(ex));
            }
            
            return result;

            #endregion
        }

        public async Task<ServiceMessage<List<Domain.Announcement.Announcement>>> GetAllAnnouncementsByAccount(ServiceMessage serviceMessage)
        {
            #region [Code]
            var result = ServiceMessage<List<Domain.Announcement.Announcement>>.CreateValidResult(serviceMessage.Identity);
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;
            result.Data = new List<Domain.Announcement.Announcement>();

            var valueRequest = $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}";
            try
            {

                do
                {
                    var queryRequest = new QueryRequest()
                    {
                        TableName = GetTable(),
                        IndexName = "vendorId-tenantId-accountId-index",
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            { "#vendorIdTenantIdAccountId", "vendorId-tenantId-accountId" }
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            { $":vendorIdTenantIdAccountId", new AttributeValue { S = $"{valueRequest}" } }
                        },
                        KeyConditionExpression = $"#vendorIdTenantIdAccountId = :vendorIdTenantIdAccountId",
                        Limit = 10,
                        ExclusiveStartKey = lastKeyEvaluated,
                        Select = Select.ALL_PROJECTED_ATTRIBUTES
                    };

                    var response = await base.GetItemByQuery(queryRequest);

                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {response.HttpStatusCode} | ConsumedCapacity: {response.ConsumedCapacity} | Metadata: {response.ResponseMetadata}", ErrorType.Technical));

                    foreach (Dictionary<string, AttributeValue> item in response.Items)
                    {
                        result.Data.Add(GetResultValue(item));
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

        public async Task<ServiceMessage<Domain.Announcement.Announcement>> GetItemById(ServiceMessage<string> serviceMessage)
        {
            #region [Code]
            var result = ServiceMessage<Domain.Announcement.Announcement>.CreateValidResult(serviceMessage.Identity);
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;

            var valueRequest = $"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Data}";
            try
            {
                var queryRequest = new QueryRequest()
                {
                    TableName = GetTable(),
                    IndexName = "vendorId-tenantId-id-index",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        { "#vendorIdTenantIdId", "vendorId-tenantId-id" }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        { $":vendorIdTenantIdId", new AttributeValue { S = $"{valueRequest}" } }
                    },
                    KeyConditionExpression = $"#vendorIdTenantIdId = :vendorIdTenantIdId",
                    Limit = 10,
                    ExclusiveStartKey = lastKeyEvaluated,
                    Select = Select.ALL_PROJECTED_ATTRIBUTES
                };

                var response = await base.GetItemByQuery(queryRequest);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {response.HttpStatusCode} | ConsumedCapacity: {response.ConsumedCapacity} | Metadata: {response.ResponseMetadata}", ErrorType.Technical));

                result.Data = (GetResultValue(response.Items.FirstOrDefault()));

                lastKeyEvaluated = response.LastEvaluatedKey;
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

