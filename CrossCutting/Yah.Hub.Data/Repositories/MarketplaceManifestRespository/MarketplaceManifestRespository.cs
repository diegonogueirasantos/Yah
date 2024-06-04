using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Repositories.JsonFile;
using Yah.Hub.Domain.Manifest;

namespace Yah.Hub.Data.Repositories.MarketplaceManifestRespository
{
    public class MarketplaceManifestRespository : AbstractJsonFileRepository, IMarketplaceManifestRespository
    {
        public MarketplaceManifestRespository(
            IConfiguration configuration,
            ILogger<MarketplaceManifest> logger, 
            Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment) 
            : base(configuration, logger, hostingEnvironment)
        {

        }

        public async Task<MarketplaceManifest> GetManifestAsync(MarketplaceServiceMessage message)
        {
            return await base.GetAsync<MarketplaceManifest>($"manifest.{message.Marketplace.ToString().ToLower()}.json");
        }
    }
}
