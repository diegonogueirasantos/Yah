using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Nest;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerApiClient;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerAuthenticationService;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCatalogService;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerCategoryService;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerMonitorService;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerOrderService;
using Yah.Hub.Application.Clients.ExternalClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.BatchItemService;
using Yah.Hub.Application.Services.IntegrationMonitorService;
using Yah.Hub.Application.Services.Manifest;
using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Application.Services.OrderService;
using Yah.Hub.Common.Services;
using Yah.Hub.Marketplace.Application.Broker;
using Yah.Hub.Marketplace.Application.Broker.Interface;

namespace Yah.Hub.Application;

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
        // add common
        new Yah.Hub.Common.Startup(this.Configuration).ConfigureServices(services);
        // add data
        new Yah.Hub.Data.Startup(this.Configuration).ConfigureServices(services);
        // amazon SNS
        services.AddAWSService<IAmazonSimpleNotificationService>();
        // amazon SQS
        services.AddAWSService<IAmazonSQS>();
        // configuration
        services.AddScoped<IBrokerConfiguration, BrokerConfiguration>();
        // broker service
        services.AddScoped<IBrokerService, BrokerService>();
        // inject services
        services.AddScoped<IERPClient, ERPClient>();
        services.AddScoped<IIntegrationMonitorService, IntegrationMonitorService>();
        services.AddScoped<IAccountConfigurationService, AccountConfigurationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMarketplaceManifestService, MarketplaceManifestService>();
        services.AddScoped<IBrokenClient, BrokenClient>();
        services.AddScoped<IBrokerCatalogService, BrokerCatalogService>();
        services.AddScoped<IBrokerCategoryService, BrokerCategoryService>();
        services.AddScoped<IBrokerAuthenticationService, BrokerAuthenticationService>();
        services.AddScoped<IBrokerOrderService, BrokerOrderService>();
        services.AddScoped<IBrokerMonitorService, BrokerMonitorService>();
        services.AddScoped<IBatchItemService, BatchItemService>();
    }
}