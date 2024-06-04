using Yah.Hub.Api.Application.Authentication;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Marketplace.Application.Authentication;

namespace Yah.Hub.Netshoes.Api.Controllers
{
    public class AuthenticationController : MarketplaceAuthenticationApi
    {
        public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger, ISecurityService securityService) : base(authenticationService, logger, securityService)
        {
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Netshoes;
        }
    }
}
