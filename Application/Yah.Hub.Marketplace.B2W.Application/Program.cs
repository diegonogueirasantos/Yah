using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.B2W.Application.Client.Interface;
using Yah.Hub.Marketplace.B2W.Application.Catalog;
using Yah.Hub.Marketplace.B2W.Application.Sales;
using Yah.Hub.Marketplace.B2W.Application.Authentication;
using Yah.Hub.Marketplace.B2W.Application.Client;

namespace Yah.Hub.Marketplace.B2W.Application;
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

        services.AddScoped<IB2WClient, B2WClient>();
        services.AddScoped<ICatalogService, B2WCatalogService>();
        services.AddScoped<ISalesService, B2WSalesService>();
        services.AddScoped<IAuthenticationService, B2WAuthenticationService>();
    }
}