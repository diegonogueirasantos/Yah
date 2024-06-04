using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;

namespace Yah.Hub.Common.Services
{
    public abstract class AbstractService
    {
        public readonly ILogger Logger;
        public readonly IConfiguration Configuration;

        private Dictionary<string, string> EnvVar = new Dictionary<string, string>() {
            {"TST", "TST" },
            { "DEV", "TST" },
            { "HLG", "HLG" },
            { "PRD", "PRD" }
        };
 

        public AbstractService(IConfiguration configuration, ILogger logger)
        {
            this.Configuration = configuration;
            this.Logger = logger;
        }


        public virtual string GetEnvironment()
        {
            return EnvVar[Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value].ToLower() ?? Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value;
        }
    }

    public abstract class AbstractMarketplaceService : AbstractService
    {
        private readonly MarketplaceAlias Marketplace;

        public AbstractMarketplaceService(IConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
        }

        public abstract MarketplaceAlias GetMarketplace();

        protected HubEnviroment HubEnviroment { get; }
    }
}

