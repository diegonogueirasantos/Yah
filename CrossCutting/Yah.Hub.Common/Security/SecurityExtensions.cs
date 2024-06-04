using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security.Identities;
using System.Security.Claims;

namespace Yah.Hub.Common.Security
{
    public static class SecurityExtensions
    {
        #region [Vendor / Tenant / Account]
        public static bool IsValidVendorTenantAccountIdentity(this Identity.Identity identity)
        {
            return identity.IsVendorTenantAccountIdentity() 
                && identity.GetVendorId() != VendorTenantAccountIdentity.EmptyClaimValue 
                && identity.GetTenantId() != VendorTenantAccountIdentity.EmptyClaimValue
                && identity.GetAccountId() != VendorTenantAccountIdentity.EmptyClaimValue;
        }


        private static bool IsVendorTenantAccountIdentity(this Identity.Identity identity)
        {
            return identity.HasClaim(VendorTenantAccountIdentity.Actor, VendorTenantAccountIdentity.TenantActor);
        }
        #endregion

        #region [Vendor / Tenant]
        public static bool IsValidVendorTenantIdentity(this Identity.Identity identity)
        {
            return identity.IsVendorTenantIdentity()
                && identity.GetVendorId() != VendorTenantAccountIdentity.EmptyClaimValue
                && identity.GetTenantId() != VendorTenantAccountIdentity.EmptyClaimValue;
        }


        private static bool IsVendorTenantIdentity(this Identity.Identity identity)
        {
            return identity.HasClaim(VendorTenantAccountIdentity.Actor, VendorTenantIdentity.TenantActor);
        }
        #endregion

        #region [Vendor]
        public static bool IsValidVendorIdentity(this Identity.Identity identity)
        {
            return identity.IsVendorIdentity()
                && identity.GetVendorId() != VendorTenantAccountIdentity.EmptyClaimValue;
        }


        private static bool IsVendorIdentity(this Identity.Identity identity)
        {
            return identity.HasClaim(VendorTenantAccountIdentity.Actor, VendorIdentity.TenantActor);
        }
        #endregion

        #region [Worker]
        public static bool IsValidWorkerIdentity(this Identity.Identity identity)
        {
            return identity.IsWorkerIdentity();
        }


        private static bool IsWorkerIdentity(this Identity.Identity identity)
        {
            return identity.HasClaim(WorkerIdentity.Actor, WorkerIdentity.WorkerActor);
        }
        #endregion

        #region [Email]
        public static bool IsValidEmailIdentity(this Identity.Identity identity)
        {
            return identity.IsEmailIdentity()
                && identity.GetMailId() != EmailIdentity.EmptyClaimValue;
        }


        private static bool IsEmailIdentity(this Identity.Identity identity)
        {
            return identity.HasClaim(EmailIdentity.Actor, EmailIdentity.EmailClaim);
        }
        #endregion

        #region [Username]
        public static bool IsValidUsernameIdentity(this Identity.Identity identity)
        {
            return identity.IsUsernameIdentity()
                && identity.GetUsername() != UsernameIdentity.EmptyClaimValue;
        }


        private static bool IsUsernameIdentity(this Identity.Identity identity)
        {
            return identity.HasClaim(UsernameIdentity.Actor, UsernameIdentity.UsernameClaim);
        }
        #endregion

        #region [Helper]

        public static string GetVendorId(this Identity.Identity identity)
        {
            return !String.IsNullOrEmpty(identity.Claims.GetValueOrDefault<string, string>(VendorTenantAccountIdentity.VendorClaim)) 
                ? identity.Claims.GetValueOrDefault<string, string>(VendorTenantAccountIdentity.VendorClaim) 
                : VendorTenantAccountIdentity.EmptyClaimValue;
        }

        public static string GetTenantId(this Identity.Identity identity)
        {
            return !String.IsNullOrEmpty(identity.Claims.GetValueOrDefault<string, string>(VendorTenantAccountIdentity.TenantClaim))
                ? identity.Claims.GetValueOrDefault<string, string>(VendorTenantAccountIdentity.TenantClaim)
                : VendorTenantAccountIdentity.EmptyClaimValue;
        }

        public static string GetAccountId(this Identity.Identity identity)
        {
            return !String.IsNullOrEmpty(identity.Claims.GetValueOrDefault<string, string>(VendorTenantAccountIdentity.AccountClaim))
                ? identity.Claims.GetValueOrDefault<string, string>(VendorTenantAccountIdentity.AccountClaim)
                : VendorTenantAccountIdentity.EmptyClaimValue;
        }

        public static string GetMailId(this Identity.Identity identity)
        {
            return !String.IsNullOrEmpty(identity.Claims.GetValueOrDefault<string, string>(EmailIdentity.EmailClaim))
                ? identity.Claims.GetValueOrDefault<string, string>(EmailIdentity.EmailClaim)
                : EmailIdentity.EmptyClaimValue;
        }

        public static string GetUsername(this Identity.Identity identity)
        {
            return !String.IsNullOrEmpty(identity.Claims.GetValueOrDefault<string, string>(UsernameIdentity.UsernameClaim))
                ? identity.Claims.GetValueOrDefault<string, string>(UsernameIdentity.UsernameClaim)
                : UsernameIdentity.EmptyClaimValue;
        }

        #endregion

    }
}
