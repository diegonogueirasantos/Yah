using System.Security.Claims;

namespace Yah.Hub.Common.Security.Identities
{
    public class VendorTenantIdentity : Identity.Identity
    {
        public const string Actor = "ClaimType";
        public const string EmptyClaimValue = "0";
        public const string TenantActor = "vendorTenant";
        public const string VendorClaim = "vendorId";
        public const string TenantClaim = "tenantId";

        public VendorTenantIdentity(string vendorId, string tenantId)
        : base(new Dictionary<string, string>() {
                { Actor, TenantActor },
                { VendorClaim,  vendorId },
                { TenantClaim,  tenantId }
                })
        { }

        public string VendorId
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(VendorClaim)) ? Claims.GetValueOrDefault<string, string>(VendorClaim) : EmptyClaimValue;
            }
        }

        public string TenantId
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(TenantClaim)) ? Claims.GetValueOrDefault<string, string>(TenantClaim) : EmptyClaimValue;
            }
        }
    }
}
