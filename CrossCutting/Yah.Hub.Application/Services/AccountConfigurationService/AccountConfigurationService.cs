using System;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.AccountConfigurationRepository;

namespace Yah.Hub.Application.Services.AccountConfigurationService
{
    public class AccountConfigurationService : AbstractService, IAccountConfigurationService
    {
        private readonly IAccountConfigurationRepository ConfigurationRepository;

        public AccountConfigurationService(IConfiguration configuration, ILogger<AccountConfigurationService> logger, IAccountConfigurationRepository configurationRepository) : base(configuration, logger)
        {
            this.ConfigurationRepository = configurationRepository;
        }

        public async Task<ServiceMessage<AccountConfiguration>> GetConfiguration(MarketplaceServiceMessage message)
        {
            if (message.Identity.IsValidEmailIdentity())
            {
                return await this.ConfigurationRepository.GetItemByEmail(new ServiceMessage<string>(message.Identity,$"{message.Identity.GetMailId()}-{message.Marketplace}"));
            }

            if (message.Identity.IsValidUsernameIdentity())
            {
                return await this.ConfigurationRepository.GetItemByUsername(new ServiceMessage<string>(message.Identity, $"{message.Identity.GetUsername()}-{message.Marketplace}"));
            }

            return await this.ConfigurationRepository.GetItemByPartitionKey(new ServiceMessage<string>(message.Identity, $"{message.Identity.GetVendorId()}-{message.Identity.GetTenantId()}-{message.Identity.GetAccountId()}-{message.Marketplace}"));
        }

        public async Task<ServiceMessage> SetConfiguration(ServiceMessage<AccountConfiguration> message)
        {
            return await this.ConfigurationRepository.UpsertItem(message);
        }

        public async Task<List<AccountConfiguration>> GetSummary()
        {
            return await this.ConfigurationRepository.GetIntegrationSummary();
        }
    }
}

