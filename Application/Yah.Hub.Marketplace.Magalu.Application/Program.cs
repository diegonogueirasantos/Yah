using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.Magalu.Application.Client.Interface;
using Yah.Hub.Marketplace.Magalu.Application.Catalog;
using Yah.Hub.Marketplace.Magalu.Application.Sales;
using Yah.Hub.Marketplace.Magalu.Application.Authentication;
using Yah.Hub.Marketplace.Magalu.Application.Client;

namespace Yah.Hub.Marketplace.Magalu.Application;
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

        services.AddScoped<IMagaluClient, MagaluClient>();
        services.AddScoped<ICatalogService, MagaluCatalogService>();
        services.AddScoped<ISalesService, MagaluSalesService>();
        services.AddScoped<IAuthenticationService, MagaluAuthenticationService>();
    }
}