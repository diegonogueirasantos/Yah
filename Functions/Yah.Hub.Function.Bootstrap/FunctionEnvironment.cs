using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yah.Hub.Function.Bootstrap
{
    public class FunctionEnvironment
    {
        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; }

        public FunctionEnvironment(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }
    }

    public class FunctionEnvironmentBootstrap
    {
        private static IConfiguration BootstrapConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                               .AddEnvironmentVariables();

            configBuilder.AddJsonFile("appsettings.json");

            return configBuilder.Build();
        }

        private static IServiceProvider BootstrapServiceProvider(IConfiguration config,Action<IConfiguration, IServiceCollection> servicesRegister = null)
        {
            var serviceCollection = new ServiceCollection();

            new Startup(config).ConfigureServices(serviceCollection);

            servicesRegister?.Invoke(config, serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        public static FunctionEnvironment Build(Action<IConfiguration, IServiceCollection> servicesRegister = null)
        {
            var config = BootstrapConfiguration();
            var serviceProvider = BootstrapServiceProvider(config, servicesRegister);

            return new FunctionEnvironment(config, serviceProvider);
        }
    }
}
