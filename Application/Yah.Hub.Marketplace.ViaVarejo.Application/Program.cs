using Yah.Hub.Marketplace.Application.Authentication;
using Yah.Hub.Marketplace.Application.Catalog;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.Application.Sales;
using Yah.Hub.Marketplace.ViaVarejo.Application.Authentication;
using Yah.Hub.Marketplace.ViaVarejo.Application.Catalog;
using Yah.Hub.Marketplace.ViaVarejo.Application.Category;
using Yah.Hub.Marketplace.ViaVarejo.Application.Category.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Client;
using Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Sales;

namespace Yah.Hub.Marketplace.ViaVarejo.Application
{
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

            services.AddScoped<IViaVarejoClient, ViaVarejoClient>();
            services.AddScoped<ICatalogService, ViaVarejoCatalogService>();
            services.AddScoped<ISalesService, ViaVarejoSalesService>();
            services.AddScoped<IAuthenticationService, ViaVarejoAuthenticationService>();
            services.AddScoped<ICategoryService, ViaVarejoCategoryService>();
        }
    }
}