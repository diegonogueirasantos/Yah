using System;
using Nest;
using System.Security.Principal;
using Yah.Hub.Common.Exceptions;
using Yah.Hub.Common.Marketplace;


namespace Yah.Hub.Common.ChannelConfiguration
{
    public class AccountConfiguration
    {
        public AccountConfiguration(string vendorId, string tenantId, string accountId)
        {
            VendorId = vendorId;
            TenantId = tenantId;
            AccountId = accountId;
        }

        public AccountConfiguration(string vendorId, string tenantId, string accountId, MarketplaceAlias marketplace)
        {
            this.Marketplace = marketplace;
            this.VendorId = vendorId;
            this.TenantId = tenantId;
            this.AccountId = accountId;
        }

        public string VendorId { get; set; }
        public string TenantId { get; set; }
        public string AccountId { get; set; }

        public bool IsActive { get; set; }
        public string? AppId { get; set; }
        public string? SecretKey { get; set; }
        public string? User { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public MarketplaceAlias Marketplace { get; set; }

        //public ExternalConfiguration ExternalConfiguration { get; set; }
    }
}

