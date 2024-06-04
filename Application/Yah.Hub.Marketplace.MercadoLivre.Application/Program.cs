using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Marketplace.Application.Announcement;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Repositories.Announcement;
using Yah.Hub.Marketplace.MercadoLivre.Application.Authorization;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog;
using Yah.Hub.Marketplace.MercadoLivre.Application.Catalog.Interface;
using Yah.Hub.Marketplace.MercadoLivre.Application.Client;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.MercadoLivre.Application.Sales;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.MercadoLivre.Application.Category;
using Yah.Hub.Data.Repositories.AnnouncementRepository;
using Yah.Hub.Application.Services.AnnouncementService;
using Yah.Hub.Marketplace.MercadoLivre.Application.Notification;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Notifications;
using Yah.Hub.Marketplace.Application.Notification;

namespace Yah.Hub.Marketplace.MercadoLivre.Application;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // adds cross cutting application
        new Yah.Hub.Marketplace.Application.Startup(this.Configuration).ConfigureServices(services);

        // adds mercadolivre services
        services.AddScoped<IMercadoLivreClient, MercadoLivreClient>();
        services.AddScoped<IAuthenticationService, MercadoLivreAuthenticationService>();
        services.AddScoped<IMercadoLivreCatalogService, MercadoLivreCatalogService>();
        services.AddScoped<ISalesService, MercadoLivreSalesService>();
        services.AddScoped<ICategoryService, MercadoLivreCategoryService>();
        services.AddScoped<IAnnouncementSearchService, AnnouncementSearchService>();
        services.AddScoped<IMercadoLivreNotificationService<MeliNotification>, MercadoLivreNotificationService>();
    }
}