using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Manifest;
using Yah.Hub.Domain.Monitor;

namespace Yah.Hub.Marketplace.Application.Validation.Interface
{
    public interface IValidationService
    {
        Task<MarketplaceServiceMessage<ProductIntegrationInfo>> Validate(MarketplaceServiceMessage<(Product Product, MarketplaceManifest Manifest)> message);
    }
}
