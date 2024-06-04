using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.Netshoes.Application.Authentication;
using Yah.Hub.Marketplace.Netshoes.Application.Catalog;
using Yah.Hub.Marketplace.Netshoes.Application.Category;
using Yah.Hub.Marketplace.Netshoes.Application.Client;
using Yah.Hub.Marketplace.Netshoes.Application.Sales;

namespace Yah.Hub.Marketplace.Netshoes.Application;
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

        services.AddScoped<INetshoesClient, NetshoesClient>();
        services.AddScoped<ICatalogService, NetshoesCatalogService>();
        services.AddScoped<ISalesService, NetshoesSalesService>();
        services.AddScoped<IAuthenticationService, NetshoesAuthenticationService>();
        services.AddScoped<ICategoryService, NetshoesCategoryService>();
    }
}