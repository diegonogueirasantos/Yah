using System;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Api.Application.Catalog;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Notification;
using System;
using Yah.Hub.Api.Application.Authentication;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Notifications;
using Yah.Hub.Common.Notification;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.MercadoLivre.Application.Notification;

namespace Yah.Hub.MercadoLivre.Api.Controllers
{
	public class NotificationController : MarketplaceNotificationApi<MeliNotification>
    {
        
		public NotificationController(IMercadoLivreNotificationService<MeliNotification> notificationService, IAuthenticationService authenticationService, ILogger<NotificationController> logger, ISecurityService securityService) : base(notificationService, authenticationService, logger, securityService)
		{
		}

        [HttpGet("SetAuthentication")]
        public virtual async Task<JsonResult> SetAuthentication()
        {
            var qs = this.Request.Query;
            if (qs.Any())
            {
                var state = this.Request.Query["state"].ToString();
                var code = this.Request.Query["code"].ToString();

                //pass this to a utils
                string vendorId = state.ExtractVendorId("-");
                string tenantId = state.ExtractTenantId("-");
                string accountId = state.ExtractAccountId("-");

                var serviceMessage = new ServiceMessage<string>(await SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), code);
                await HandleAction(() => this.AuthenticationService.SetAuthentication(serviceMessage));
            }
            return new JsonResult(new { });
        }

        [HttpPost("Notification")]
        public virtual async Task<JsonResult> Notification(MeliNotification notification)
        {
            var accountConfigResult = await this.AuthenticationService.GetAccountConfiguration(new ServiceMessage(await SecurityService.IssueUsernameIdentity(notification.UserId.ToString())));
            var serviceMessage = new ServiceMessage<string>(await SecurityService.IssueVendorTenantAccountIdentity(accountConfigResult.Data.VendorId, accountConfigResult.Data.TenantId, accountConfigResult.Data.AccountId));
            var notificationEvent = new NotificationEvent<MeliNotification>();

            notificationEvent.Data = notification;
            notificationEvent.EntityType = Common.Enums.EntityType.Announcement;
            notificationEvent.EventDateTime = DateTime.Now;

            await HandleAction(() => this.NotificationService.EnqueueNotificationCommand(notificationEvent.AsServiceMessage(serviceMessage.Identity)));
         
            return new JsonResult(new { });
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }
    }
}