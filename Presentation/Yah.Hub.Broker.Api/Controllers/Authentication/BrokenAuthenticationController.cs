using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Application.Broker.BrokerServiceApi.BrokerAuthenticationService;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Broker.Api.Controllers.Authentication
{
    public class BrokenAuthenticationController : ControllerBase
    {
        private IBrokerAuthenticationService BrokerAuthenticationService;
        private ISecurityService SecurityService;
        private ILogger Logger;

        public BrokenAuthenticationController(IBrokerAuthenticationService brokerAuthenticationService, ISecurityService securityService, ILogger<BrokenAuthenticationController> logger)
        {
            BrokerAuthenticationService = brokerAuthenticationService;
            SecurityService = securityService;
            Logger = logger;
        }

        /// <summary>
        /// Cria e ou atualiza as informações de um conta
        /// </summary>
        /// <returns></returns>
        [HttpPost("authentication/saveconfigurations")]
        public async Task<ServiceMessage> SaveAccountConfiguration(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "user")] string user,
            [FromHeader(Name = "email")] string email,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerAuthenticationService.SaveAccountConfiguration(
                new MarketplaceServiceMessage<Dictionary<string, string>>(
                    await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace,
                    new Dictionary<string, string>() { { "user", user }, { "email", email } })
                );
        }

        /// <summary>
        /// Valida se as credenciais de um marketplace são válidas
        /// </summary>
        /// <returns></returns>
        [HttpGet("authentication/validadecredentials")]
        public async Task<ServiceMessage> ValidadeCredentials(
            [FromHeader(Name = "x-VendorId")] string vendorId,
            [FromHeader(Name = "x-TenantId")] string tenantId,
            [FromHeader(Name = "x-AccountId")] string accountId,
            [FromHeader(Name = "x-Marketplace")] MarketplaceAlias marketplace)
        {
            return await this.BrokerAuthenticationService.ValidadeCredentials(
                new MarketplaceServiceMessage( await this.SecurityService.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId), marketplace));
        }
    }
}
