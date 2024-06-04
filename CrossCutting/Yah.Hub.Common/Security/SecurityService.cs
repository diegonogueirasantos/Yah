using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Security.Identities;
using Yah.Hub.Common.Services;
using System.Security.Claims;

namespace Yah.Hub.Common.Security
{
    public class SecurityService : AbstractService, ISecurityService
    {
        public SecurityService(IConfiguration configuration, ILogger<SecurityService> logger) : base(configuration, logger)
        {
        }

        public async Task<Identity.Identity> IssueVendorTenantAccountIdentity(string vendorId, string tenantId, string accountId)
        {
            var identity = new VendorTenantAccountIdentity(vendorId, tenantId, accountId);
            return identity;
        }

        public async Task<Identity.Identity> IssueVendorTenantIdentity(string vendorId, string tenantId)
        {
            var identity = new VendorTenantIdentity(vendorId, tenantId);
            return identity;
        }

        public async Task<Identity.Identity> IssueVendorIdentity(string tenantId)
        {
            var identity = new VendorIdentity(tenantId);
            return identity;
        }

        public async Task<Identity.Identity> IssueWorkerIdentity()
        {
            var identity = new WorkerIdentity();
            return identity;
        }

        public async Task<Identity.Identity> ImpersonateClaimIdentity(Identity.Identity claim, string vendorId, string tenantId = null, string accountId = null)
        {
            #region [Code]
            if (!(claim.IsValidWorkerIdentity() || claim.IsValidEmailIdentity() || claim.IsValidUsernameIdentity()))
            {
                throw new InvalidOperationException();
            }

            if (!String.IsNullOrEmpty(vendorId) && !String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(accountId))
            {
                return await this.IssueVendorTenantAccountIdentity(vendorId, tenantId, accountId);
            }

            if (!String.IsNullOrEmpty(vendorId) && !String.IsNullOrEmpty(tenantId) && String.IsNullOrEmpty(accountId))
            {
                return await this.IssueVendorTenantIdentity(vendorId, tenantId);
            }

            if (!String.IsNullOrEmpty(vendorId) && String.IsNullOrEmpty(tenantId) && String.IsNullOrEmpty(accountId))
            {
                return await this.IssueVendorIdentity(vendorId);
            }

            return await this.IssueWorkerIdentity();
            #endregion
        }

        public async Task<Identity.Identity> IssueEmailIdentity(string mail)
        {
            var identity = new EmailIdentity(mail);

            return identity;
        }

        public async Task<Identity.Identity> IssueUsernameIdentity(string mail)
        {
            var identity = new UsernameIdentity(mail);

            return identity;
        }
    }
}
