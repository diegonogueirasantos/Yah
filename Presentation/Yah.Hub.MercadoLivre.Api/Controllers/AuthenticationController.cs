using System;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Api.Application.Authentication;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Authentication;

namespace Yah.Hub.MercadoLivre.Api.Controllers
{
    public class AuthenticationController : MarketplaceAuthenticationApi
    {
        private IAuthenticationService AuthenticationService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public AuthenticationController(IAuthenticationService autenticationService, ILogger<AuthenticationController> logger, ISecurityService securityService) : base(autenticationService, logger, securityService)
        {
            AuthenticationService = autenticationService;
            Logger = logger;
            SecurityService = securityService;
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }
    }
}

