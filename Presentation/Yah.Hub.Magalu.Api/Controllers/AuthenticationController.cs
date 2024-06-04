using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yah.Hub.Api.Application.Authentication;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.Application.Authentication;

namespace Yah.Hub.Magalu.Api.Controllers
{
    public class AuthenticationController : MarketplaceAuthenticationApi
    {
        private IAuthenticationService AuthenticationService;
        private ILogger Logger;
        private ISecurityService SecurityService;

        public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger, ISecurityService securityService) : base(authenticationService, logger, securityService)
        {
            AuthenticationService = authenticationService;
            Logger = logger;
            SecurityService = securityService;
        }

        [HttpGet("Callback")]
        public virtual async Task<JsonResult> SetAuthentication()
        {
            var qs = this.Request.Query;

            string state = this.Request.Query["state"].ToString();
            string cnpj = state.Split("&").First();
            string code = this.Request.Query["code"].ToString();
            string identifier = string.Empty;

            var parameters = new Dictionary<string, string>();

            foreach(var param in state.Split("&").Skip(1))
            {
                parameters.Add(param.Split("=").First(), param.Split("=").Last());
            }

            if (qs.Any())
            {
                if (parameters.Where(x => x.Key == "type").First().Value == "migration")
                {
                    identifier = parameters.Where(x => x.Key == "username").First().Value;

                    var serviceMessage = new ServiceMessage<string>(await SecurityService.IssueUsernameIdentity(identifier), code);
                    await HandleAction(() => this.AuthenticationService.SetAuthentication(serviceMessage));
                }
                else
                {
                    identifier = parameters.Where(x => x.Key == "email").First().Value;

                    var serviceMessage = new ServiceMessage<string>(await SecurityService.IssueEmailIdentity(identifier), code);
                    await HandleAction(() => this.AuthenticationService.SetAuthentication(serviceMessage));

                }

            }

            return new JsonResult(new { });
        }

        protected override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Magalu;
        }
    }
}
