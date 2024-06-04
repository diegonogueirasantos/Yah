using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Data.Repositories.AccountConfigurationRepository;
using Yah.Hub.Data.Repositories.BatchItemRepository;
using Yah.Hub.Data.Repositories.IntegrationMonitorRepository;
using Yah.Hub.Data.Repositories.MarketplaceManifestRespository;
using Yah.Hub.Data.Repositories.OrderRepository;

namespace Yah.Hub.Data;

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
        services.AddScoped<IIntegrationMonitorRepository, IntegrationMonitorRepository>();
        services.AddScoped<IAccountConfigurationRepository, AccountConfigurationRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IMarketplaceManifestRespository, MarketplaceManifestRespository>();
        services.AddScoped<IBatchItemRepository, BatchItemRepository>();
    }
}