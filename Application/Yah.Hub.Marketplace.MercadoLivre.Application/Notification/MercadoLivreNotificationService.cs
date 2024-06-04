using System;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Notification;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Notification;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Notifications;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Notification
{
	public class MercadoLivreNotificationService : NotificationService<MeliNotification>, IMercadoLivreNotificationService<MeliNotification>
	{
        private IMercadoLivreCatalogService catalogService { get; set; }

        public MercadoLivreNotificationService(IConfiguration configuration,
			ILogger<NotificationService<MeliNotification>> logger, IBrokerService brokerService, IMercadoLivreCatalogService mercadoLivreCatalogService, IAccountConfigurationService accountConfigurationService) : base(configuration, logger, brokerService, accountConfigurationService)
		{
            this.catalogService = mercadoLivreCatalogService;
		}

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }

        public override async Task<ServiceMessage> ProcessNotification(MarketplaceServiceMessage<NotificationEvent<MeliNotification>> notificationEvent)
        {
            var result = new ServiceMessage(notificationEvent.Identity);

            var accountConfigResult = await base.ConfigurationService.GetConfiguration(notificationEvent);

            switch (notificationEvent.Data.Data.Topic)
            {
                case "items":
                    result = await this.catalogService.ResyncAnnouncement(notificationEvent.Data.Data.Resource.Split("/").Last().AsMarketplaceServiceMessage(notificationEvent.Identity, accountConfigResult.Data));
                    break;

                // for knowledge purposes
                case "orders_v2":
                    break;
                case "questions":
                    break;
                case "payments":
                    break;
                case "messages":
                    break;
                case "quotations":
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}

