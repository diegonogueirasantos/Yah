using System;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Notification;

namespace Yah.Hub.Api.Application.Catalog
{
    public abstract class MarketplaceNotificationApi<T> : MarketplaceControllerBase.MarketplaceControllerBase
    {
        protected INotificationService<T> NotificationService;
        protected IAuthenticationService AuthenticationService;
        private ILogger Logger;
        protected ISecurityService SecurityService;

        public MarketplaceNotificationApi(INotificationService<T> notificationService, IAuthenticationService authenticationService, ILogger logger, ISecurityService securityService)
        {
            this.AuthenticationService = authenticationService;
            this.NotificationService = notificationService;
            this.Logger = logger;
            this.SecurityService = securityService;
        }

        [HttpGet("ProcessNotificationCommands")]
        public virtual async Task<IServiceMessage> ProcessNotificationCommands(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId)
        {
            return await HandleAction(() => this.NotificationService.ConsumeNotificationCommands(new ServiceMessage(SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId).GetAwaiter().GetResult())));
        }
    }
}