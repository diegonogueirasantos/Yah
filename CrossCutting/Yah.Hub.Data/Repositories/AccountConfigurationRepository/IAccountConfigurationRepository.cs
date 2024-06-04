using System;
using Yah.Hub.Common.AbstractRepositories.DynamoDB;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Data.Repositories.AccountConfigurationRepository
{
    public interface IAccountConfigurationRepository : IDynamoRepository<AccountConfiguration>
    {
        public Task<List<AccountConfiguration>> GetIntegrationSummary();
        public Task<ServiceMessage<AccountConfiguration>> GetItemByEmail(ServiceMessage<string> serviceMessage);
        public Task<ServiceMessage<AccountConfiguration>> GetItemByUsername(ServiceMessage<string> serviceMessage);
    }
}

