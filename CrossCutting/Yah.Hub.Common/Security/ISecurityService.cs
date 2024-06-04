namespace Yah.Hub.Common.Security
{
    public interface ISecurityService
    {
        public Task<Identity.Identity> ImpersonateClaimIdentity(Identity.Identity workerIdentity, string vendorId, string tenantId = null, string accountId = null);
        public Task<Identity.Identity> IssueVendorTenantAccountIdentity(string vendorId, string tenantId, string accountId);
        public Task<Identity.Identity> IssueVendorTenantIdentity(string vendorId, string tenantId);
        public Task<Identity.Identity> IssueVendorIdentity(string tenantId);
        public Task<Identity.Identity> IssueEmailIdentity(string tenantId);
        public Task<Identity.Identity> IssueUsernameIdentity(string username);
        public Task<Identity.Identity> IssueWorkerIdentity();
    }
}
