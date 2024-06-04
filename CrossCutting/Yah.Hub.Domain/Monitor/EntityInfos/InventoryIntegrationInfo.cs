using System;
namespace Yah.Hub.Domain.Monitor.EntityInfos
{
	public class InventoryIntegrationInfo : BaseIntegrationInfo
	{
        public string Id { get; set; }

        public string ReferenceId { get; set; }

        /// <summary>
        /// Product stock balance.
        /// </summary>
        public int Balance { get; set; }

        /// <summary>
        /// Product preparation time for shipment
        /// </summary>
        public int? HandlingDays { get; set; }
    }
}