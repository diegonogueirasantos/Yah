using System.Security.Claims;

namespace Yah.Hub.Common.Security.Identities
{
    public class VendorIdentity : Identity.Identity
    {

        public const string EmptyClaimValue = "0";
        public const string Actor = "ClaimType";
        public const string TenantActor = "vendor";
        public const string VendorClaim = "vendorId";

        public VendorIdentity(string vendorId)
        : base(new Dictionary<string, string>() {
                { Actor, TenantActor },
                { VendorClaim,   vendorId }
                })
        { }

        public string VendorId
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(VendorClaim)) ? Claims.GetValueOrDefault<string, string>(VendorClaim) : EmptyClaimValue;
            }
        }
    }
}
