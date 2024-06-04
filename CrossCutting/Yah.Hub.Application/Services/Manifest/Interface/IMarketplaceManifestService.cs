using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Manifest;

namespace Yah.Hub.Application.Services.Manifest.Interface
{
    public interface IMarketplaceManifestService
    {
        Task<MarketplaceManifest> GetManifestAsync(MarketplaceServiceMessage message);
    }
}
