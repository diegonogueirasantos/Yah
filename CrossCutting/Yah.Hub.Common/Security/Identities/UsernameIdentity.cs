using System.Security.Claims;

namespace Yah.Hub.Common.Security.Identities
{
    public class UsernameIdentity : Identity.Identity
    {
        public const string Actor = "ClaimType";
        public const string EmptyClaimValue = "0";
        public const string UsernameClaim = "Username";
        public const string TenantActor = "Username";

        public UsernameIdentity(string mail)
            : base(new Dictionary<string, string>() {
                { Actor, TenantActor },
                { UsernameClaim,   mail }
            })
        { }

        public string Username
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(UsernameClaim)) ? Claims.GetValueOrDefault<string, string>(UsernameClaim) : EmptyClaimValue;
            }
        }
    }
}
