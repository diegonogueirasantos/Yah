namespace Yah.Hub.Domain.Catalog
{
    public class Inventory
    {
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
