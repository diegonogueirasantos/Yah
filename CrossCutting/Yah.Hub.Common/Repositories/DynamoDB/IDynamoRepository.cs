using System;
using Amazon.DynamoDBv2.Model;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Common.AbstractRepositories.DynamoDB
{
    public interface IDynamoRepository<T>
    {
        public Task<ServiceMessage.ServiceMessage> UpsertItem(ServiceMessage<T> serviceMessage);
        public Task<ServiceMessage<T>> GetItemByPartitionKey(ServiceMessage<string> serviceMessage);
        public Task<ServiceMessage<List<T>>> GetItemBySecundaryKey(ServiceMessage<string> serviceMessage);
        public Task<QueryResponse> GetItemByQuery(QueryRequest query);
    }
}

