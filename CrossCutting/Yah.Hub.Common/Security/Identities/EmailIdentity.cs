namespace Yah.Hub.Common.Security.Identities
{
    public class EmailIdentity : Identity.Identity
    {
        public const string Actor = "ClaimType";
        public const string EmptyClaimValue = "0";
        public const string EmailClaim = "Mail";
        public const string TenantActor = "Mail";

        public EmailIdentity(string mail)
            : base(new Dictionary<string, string>() {
                { Actor, TenantActor },
                { EmailClaim,   mail }
            })
        { }

        public string Email
        {
            get
            {
                return !String.IsNullOrEmpty(Claims.GetValueOrDefault<string, string>(EmailClaim)) ? Claims.GetValueOrDefault<string, string>(EmailClaim) : EmptyClaimValue;
            }
        }
    }
}
