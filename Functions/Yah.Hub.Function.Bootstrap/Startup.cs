using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Yah.Hub.Function.Bootstrap
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            new Hub.Application.Startup(Configuration).ConfigureServices(services);
        }
    }
}