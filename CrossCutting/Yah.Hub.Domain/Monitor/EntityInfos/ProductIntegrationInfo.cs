using System;
using Yah.Hub.Common.Enums;

namespace Yah.Hub.Domain.Monitor
{
	public class ProductIntegrationInfo : BaseIntegrationInfo
	{
        public ProductIntegrationInfo(string id, string referenceId, EntityStatus status, DateTimeOffset dateTime)
		{
			this.Id = id;
			this.ReferenceId = referenceId;
			this.Status = status;
			this.DateTime = dateTime;
		}

        public string Id { get; set; }
		public string ReferenceId { get; set; }
		public string IntegrationId { get; set; }
		public string MarketplaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<SkuIntegrationInfo> Skus { get; set; } = new List<SkuIntegrationInfo>();
		
	}
}

