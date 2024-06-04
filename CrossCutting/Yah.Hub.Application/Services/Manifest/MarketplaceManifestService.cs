using Yah.Hub.Application.Services.Manifest.Interface;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.MarketplaceManifestRespository;
using Yah.Hub.Domain.Manifest;

namespace Yah.Hub.Application.Services.Manifest
{
    public class MarketplaceManifestService : AbstractService, IMarketplaceManifestService
    {
        protected IMarketplaceManifestRespository MarketplaceManifestRespository { get; set; }

        public MarketplaceManifestService(
            IConfiguration configuration, 
            ILogger<MarketplaceManifestService> logger,
            IMarketplaceManifestRespository marketplaceManifestRespository) : base(configuration, logger)
        {
            this.MarketplaceManifestRespository = marketplaceManifestRespository;
        }

        public async Task<MarketplaceManifest> GetManifestAsync(MarketplaceServiceMessage message)
        {
            return await this.MarketplaceManifestRespository.GetManifestAsync(message);
        }
    }
}
