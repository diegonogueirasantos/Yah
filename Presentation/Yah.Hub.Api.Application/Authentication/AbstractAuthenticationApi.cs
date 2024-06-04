using System;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.ChannelConfiguration;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.Application.Authentication;

namespace Yah.Hub.Api.Application.Authentication
{
    public abstract class MarketplaceAuthenticationApi : MarketplaceControllerBase.MarketplaceControllerBase
    {
        private IAuthenticationService AuthenticationService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public MarketplaceAuthenticationApi(IAuthenticationService authenticationService, ILogger logger, ISecurityService securityService)
        {
            this.AuthenticationService = authenticationService;
            this.Logger = logger;
            this.SecurityService = securityService;
        }

        [HttpGet("GetAccountConfiguration")]
        public virtual async Task<IServiceMessage> GetAccountConfiguration(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AuthenticationService.GetAccountConfiguration(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpGet("GetAuthorizationUrl")]
        public virtual async Task<IServiceMessage> GetAuthorizationUrl(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            var serviceMessage = new ServiceMessage(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId));

            return await HandleAction(() => this.AuthenticationService.GetAuthorizationUrl(serviceMessage));
        }

        [HttpGet("RenewToken")]
        public virtual async Task<IServiceMessage> RenewToken(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AuthenticationService.RenewToken(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }

        [HttpPost("SaveAccountConfiguration")]
        public virtual async Task<IServiceMessage> SaveAccountConfiguration(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "user")] string user,
            [FromHeader(Name = "email")] string email,
            [FromHeader(Name = "accessToken")] string accessToken)
        {
            return await HandleAction(() => this.AuthenticationService.SaveAccountConfiguration(new ServiceMessage<AccountConfiguration>(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult(), new AccountConfiguration(vendorId, tenantId, accountId,GetMarketplace()) { User = user, Email = email, IsActive = true, AccessToken = accessToken })));
        }

        [HttpGet("ValidadeCredentials")]
        public virtual async Task<IServiceMessage> ValidadeCredentials(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.AuthenticationService.ValidateCredentials(new ServiceMessage( SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }
    }
}