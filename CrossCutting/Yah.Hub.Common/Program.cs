using System;
using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Yah.Hub.Common.Services.CacheService;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using StackExchange.Redis;
using Yah.Hub.Application.Services.ThrottlingService;
using Yah.Hub.Common.Repositories.JsonFile;
using Yah.Hub.Common.Security;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog.Formatting;

namespace Yah.Hub.Common
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            services.AddSingleton(this.Configuration);

            // DynamoDBClient
            services.AddSingleton(new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { ServiceURL = this.Configuration["Dynamo:Uri"] }));

            var connSettings = new ConnectionSettings(new Uri(this.Configuration["Elastic:Uri"])).DisableDirectStreaming(true);
            connSettings = connSettings.BasicAuthentication(this.Configuration["Elastic:User"], this.Configuration["Elastic:Pass"]);

            // ElasticClient
            services.AddSingleton<IElasticClient, ElasticClient>(x => new ElasticClient(connSettings));

            // RedisClient
            services.AddSingleton<ICacheService>(provider => new CacheService(ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints =
                {
                    Configuration["Redis:Uri"]
                },
                SyncTimeout = 5000,
                ConnectRetry = 3
            }), Configuration));

            // Throttling Service
            services.AddScoped<IThrottlingService, ThrottlingService>();
            
            // Security
            services.AddScoped<ISecurityService, SecurityService>();

            // Log
            //ConfigureLogging(environment, this.Configuration);
        }

        #region Private

        public static void ConfigureLogging(string environment, WebApplicationBuilder builder)
        {
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Logging:Elastic:Uri"]))
            {
                ModifyConnectionSettings = x => x.BasicAuthentication(builder.Configuration["Logging:Elastic:User"], builder.Configuration["Logging:Elastic:Pass"]),
                AutoRegisterTemplate = true,
                TypeName = null,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                IndexFormat = $"{Assembly.GetEntryAssembly().GetName().Name!.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}"+"-{0:yyyy-MM-dd}",
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information,
            })
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

            builder.Host.UseSerilog();

        }

        public class SOFormatter : ITextFormatter
        {
            public void Format(LogEvent logEvent, TextWriter output)
            {
                output.Write("{");
                foreach (var p in logEvent.Properties)
                {
                    output.Write("\"{0}\" : {1}, ", p.Key, p.Value);
                }
                output.Write("}");
            }
        }

        #endregion
    }
}

