using System;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;

namespace Yah.Hub.Domain.Monitor
{
	public class IntegrationSummary : MarketplaceIdentity
	{
        public IntegrationSummary(string vendorId, string tenantId, string accountId, MarketplaceAlias marketplace) : base(marketplace)
        {
			this.VendorId = vendorId;
			this.TenantId = tenantId;
			this.AccountId = accountId;
        }

        public Int64 Waiting { get; set; }
        public Int64 Unknown { get; set; }
        public Int64 PendingValidation { get; set; }
		public Int64 Declined { get; set; }
		public Int64 Accepted { get; set; }
		public Int64 Stopped { get; set; }
		public Int64 Closed { get; set; }
        public Int64 Paused { get; set; }
        public string VendorId { get; private set; }
		public string TenantId { get; private set; }
		public string AccountId { get; private set; }
	}
}

