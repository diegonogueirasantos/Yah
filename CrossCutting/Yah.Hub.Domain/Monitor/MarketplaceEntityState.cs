using System;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Domain.Monitor
{
	public class MarketplaceEntityState : BaseEntity 
	{
        #region Identity Props

        public string VendorId { get; set; }
        public string TenantId { get; set; }
        public string AccountId { get; set; }
        public MarketplaceAlias MarketplaceAlias { get; set; }

        #endregion

        #region Constructors

        public MarketplaceEntityState(string id, string referenceId, DateTimeOffset dateTime, string vendorId, string tenantId, string accountId, MarketplaceAlias marketplace)
		{
            this.DateTime = dateTime;
			this.Id = id;
            this.MarketplaceAlias = marketplace;
            this.VendorId = vendorId;
            this.TenantId = tenantId;
            this.AccountId = accountId;
            this.ReferenceId = referenceId;
        }

        #endregion

        public string ReferenceId { get; set; }
		public DateTimeOffset DateTime { get; set; }
        public ProductIntegrationInfo ProductInfo { get; set; }
    }
}