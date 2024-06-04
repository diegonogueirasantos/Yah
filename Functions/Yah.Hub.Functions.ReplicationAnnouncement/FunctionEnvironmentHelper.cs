using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yah.Hub.Function.Bootstrap;
using Yah.Hub.Functions.ReplicationAnnouncement.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yah.Hub.Functions.ReplicationAnnouncement
{
    public static class FunctionEnvironmentHelper
    {
        public static FunctionEnvironment Build()
        {
            return FunctionEnvironmentBootstrap.Build(ConfigureServices);
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            new Yah.Hub.Application.Startup(configuration).ConfigureServices(services);

            services.AddTransient<IReplicationAnnouncementHandler, ReplicationAnnouncementHandler>();
        }
    }
}
