using System;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Data.Repositories.AccountConfigurationRepository;

namespace Yah.Hub.Application.Services.AccountConfigurationService
{
    public interface IAccountConfigurationService
    {
        public Task<ServiceMessage<AccountConfiguration>> GetConfiguration(MarketplaceServiceMessage message);
        public Task<ServiceMessage> SetConfiguration(ServiceMessage<AccountConfiguration> message);
        public Task<List<AccountConfiguration>> GetSummary();
    }
}

