using System;
using Yah.Hub.Domain.Monitor.EntityInfos;

namespace Yah.Hub.Domain.Monitor
{
	public class SkuIntegrationInfo : BaseIntegrationInfo
	{
		public string Sku { get; set; }
		public string ParentSku { get; set; }
		public string Name { get; set; }
		public string IntegrationId { get; set; }

		public PriceIntegrationInfo PriceIntegrationInfo { get; set; }
		public InventoryIntegrationInfo InventoryIntegrationInfo { get; set; }
	}
}

