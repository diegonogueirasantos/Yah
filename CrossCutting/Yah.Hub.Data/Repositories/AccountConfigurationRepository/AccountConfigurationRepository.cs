using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Nest;
using Yah.Hub.Common.AbstractRepositories.DynamoDB;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Data.Repositories.AccountConfigurationRepository
{
    public class AccountConfigurationRepository : AbstractDynamoRepository<AccountConfiguration>, IAccountConfigurationRepository
    {

        public AccountConfigurationRepository(AmazonDynamoDBClient client, ILogger<AccountConfigurationRepository> logger, IConfiguration configuration) : base(client, configuration, logger)
        {
        }

        public override Dictionary<string, AttributeValue> GetEntityKeys(ServiceMessage<AccountConfiguration> serviceMessage)
        {
            var attributes = new Dictionary<string, AttributeValue>()
            {
                { "accessToken", new AttributeValue { S = serviceMessage.Data.AccessToken ?? string.Empty }},
                { "appId", new AttributeValue { S = serviceMessage.Data.AppId ?? string.Empty }},
                { "isActive", new AttributeValue { BOOL = serviceMessage.Data.IsActive }},
                { "secretKey", new AttributeValue { S = serviceMessage.Data.SecretKey ?? string.Empty }},
                { "refreshToken", new AttributeValue { S = serviceMessage.Data.RefreshToken ?? string.Empty }},
                { "user", new AttributeValue { S = serviceMessage.Data.User ?? string.Empty }},
                { "password", new AttributeValue { S = serviceMessage.Data.Password ?? string.Empty }},
                { "email", new AttributeValue { S = serviceMessage.Data.Email ?? string.Empty }},
                { "marketplace", new AttributeValue { S = serviceMessage.Data.Marketplace.ToString()}},
                { "email-marketplace", new AttributeValue { S = $"{serviceMessage.Data.Email ?? $"{this.DefaultEmail(serviceMessage)}"}-{serviceMessage.Data.Marketplace.ToString()}"}}
            };

            if (String.IsNullOrEmpty(serviceMessage.Data.User))
            {
                attributes.Add("username-marketplace", new AttributeValue { S = $"{this.DefaultEmail(serviceMessage)}-{serviceMessage.Data.Marketplace.ToString()}" });
            }
            else
            {
                attributes.Add("username-marketplace", new AttributeValue { S = $"{serviceMessage.Data.User}-{serviceMessage.Data.Marketplace.ToString()}" });
            }

            return attributes;
        }

        public override string GetPartitionKey()
        {
            return "vendorId-tenantId-accountId-marketplaceName";
        }

        public override string GetTableName()
        {
            // TODO: dynamic environment based on app.settings
            return $"account_configurations";
        }

        public override string GetSecundaryKey()
        {
           return "vendorId-tenantId";
        }

        public override AttributeValue GetPartitionKeyValue(ServiceMessage<AccountConfiguration> serviceMessage)
        {
            return new AttributeValue($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}-{serviceMessage.Identity.GetAccountId()}-{serviceMessage.Data.Marketplace.ToString()}");
        }

        public override AccountConfiguration GetResultValue(GetItemResponse response)
        {
            return new AccountConfiguration(response.Item["vendorId"].S, response.Item["tenantId"].S, response.Item["accountId"].S)
            {
                AccessToken = response.Item["accessToken"].S,
                AppId = response.Item["appId"].S,
                IsActive = response.Item["isActive"].BOOL,
                RefreshToken = response.Item["refreshToken"].S,
                SecretKey = response.Item["secretKey"].S,
                Email = response.Item["email"].S,
                Password = response.Item["password"].S,
                User = response.Item["user"].S, 
                Marketplace =  (MarketplaceAlias)Enum.Parse(typeof(MarketplaceAlias),response.Item["marketplace"].S, true)
            };
        }

        public override AttributeValue GetSecundaryKeyValue(ServiceMessage<AccountConfiguration> serviceMessage)
        {
            return new AttributeValue($"{serviceMessage.Identity.GetVendorId()}-{serviceMessage.Identity.GetTenantId()}");
        }

        protected override AttributeValue GetSortKeyValue(ServiceMessage<AccountConfiguration> entity)
        {
            throw new NotImplementedException();
        }


        public async Task<List<AccountConfiguration>> GetIntegrationSummary()
        {
            var result = new List<AccountConfiguration>();
            Dictionary<string, AttributeValue> lastKeyEvaluated = null;

            try
            {
                do
                {
                    var queryRequest = new ScanRequest()
                    {
                        TableName = GetTable(),
                        Limit = 10,
                        ExclusiveStartKey = lastKeyEvaluated,
                    };

                    var response = await base.GetItemByScan(queryRequest);

                    //if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    //    result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {response.HttpStatusCode} | ConsumedCapacity: {response.ConsumedCapacity} | Metadata: {response.ResponseMetadata}", ErrorType.Technical));

                    foreach (Dictionary<string, AttributeValue> item in response.Items)
                    {
                        result.Add(GetResultValue(item));
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
        }

        public async Task<ServiceMessage<AccountConfiguration>> GetItemByEmail(ServiceMessage<string> serviceMessage)
        {
            var result = ServiceMessage<AccountConfiguration>.CreateValidResult(serviceMessage.Identity);

            try
            {
                var request = new QueryRequest()
                {
                    TableName = GetTable(),
                    IndexName = "email-marketplace-index",
                    ExpressionAttributeNames = new Dictionary<string, string>() { { "#kn0", "email-marketplace" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>() { { ":kv0", new AttributeValue(serviceMessage.Data) } },
                    KeyConditionExpression = "#kn0 = :kv0",
                    Select = "ALL_PROJECTED_ATTRIBUTES"
                };

                var getResult = await base.GetItemByQuery(request);

                if (getResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {getResult.HttpStatusCode} | ConsumedCapacity: {getResult.ConsumedCapacity} | Metadata: {getResult.ResponseMetadata}", ErrorType.Technical));


                result.WithData(GetResultValue(getResult.Items.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        public async Task<ServiceMessage<AccountConfiguration>> GetItemByUsername(ServiceMessage<string> serviceMessage)
        {
            var result = ServiceMessage<AccountConfiguration>.CreateValidResult(serviceMessage.Identity);

            try
            {
                var request = new QueryRequest()
                {
                    TableName = GetTable(),
                    IndexName = "username-marketplace-index",
                    ExpressionAttributeNames = new Dictionary<string, string>() { { "#kn0", "username-marketplace" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>() { { ":kv0", new AttributeValue(serviceMessage.Data) } },
                    KeyConditionExpression = "#kn0 = :kv0",
                    Select = "ALL_PROJECTED_ATTRIBUTES"
                };

                var getResult = await base.GetItemByQuery(request);

                if (getResult.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    result.WithError(new Error("Error on GET entity on DynamoDB", $"HTTP Status Code: {getResult.HttpStatusCode} | ConsumedCapacity: {getResult.ConsumedCapacity} | Metadata: {getResult.ResponseMetadata}", ErrorType.Technical));


                result.WithData(GetResultValue(getResult.Items.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                result.WithError(new Error(ex));
            }

            return result;
        }

        private string DefaultEmail(ServiceMessage<AccountConfiguration> serviceMessage)
        {
            var identity = serviceMessage.Identity;

            return $"{identity.GetVendorId()}-{identity.GetTenantId()}-{identity.GetAccountId()}@Yahhub.com.br";
        }
    }
}

