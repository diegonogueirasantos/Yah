using System;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Marketplace.Application.Authentication
{
    public interface IAuthenticationService
    {
        public Task<ServiceMessage> SetAuthentication(ServiceMessage<string> serviceMessage);
        public Task<ServiceMessage> RenewToken(ServiceMessage serviceMessage);
        public Task<ServiceMessage<string>> GetAuthorizationUrl(ServiceMessage serviceMessage);

        public Task<ServiceMessage<AccountConfiguration>> GetAccountConfiguration(ServiceMessage serviceMessage);
        public Task<ServiceMessage<AccountConfiguration>> SaveAccountConfiguration(ServiceMessage<AccountConfiguration> configuration);
        public Task<ServiceMessage> ValidateCredentials(ServiceMessage serviceMessage);


    }
}

