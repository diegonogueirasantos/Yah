using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Manifest;

namespace Yah.Hub.Data.Repositories.MarketplaceManifestRespository
{
    public interface IMarketplaceManifestRespository
    {
        Task<MarketplaceManifest> GetManifestAsync(MarketplaceServiceMessage message);
    }
}
