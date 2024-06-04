using System.Security.Claims;

namespace Yah.Hub.Common.Security.Identities
{
    public class VendorTenantAccountIdentity : Identity.Identity
    {
        public const string Actor = "ClaimType";
        public const string EmptyClaimValue = "0";
        public const string TenantActor = "vendorTenantAccount";
        public const string TenantClaim = "tenantId";
        public const string VendorClaim = "vendorId";
        public const string AccountClaim = "accountId";

        public VendorTenantAccountIdentity(string vendorId, string tenantId, string accountId)
        : base(new Dictionary<string, string>() {
                { Actor, TenantActor },
                { VendorClaim,   vendorId },
                { TenantClaim,   tenantId },
                { AccountClaim,   accountId }
                })
        { }

        public string VendorId
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string,string>(VendorClaim)) ? Claims.GetValueOrDefault<string, string>(VendorClaim) : EmptyClaimValue;
            }
        }

        public string TenantId
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(TenantClaim)) ? Claims.GetValueOrDefault<string, string>(TenantClaim) : EmptyClaimValue;
            }
        }

        public string AccountId
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(AccountClaim)) ? Claims.GetValueOrDefault<string, string>(AccountClaim) : EmptyClaimValue;
            }
        }
    }
}
