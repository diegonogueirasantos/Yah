using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluShipmentLabel
    {
        [JsonProperty("Url")]
        public string Url { get; set; }
        [JsonProperty("ExpiresOn")]
        public DateTime ExpiresOn { get; set; }
        [JsonProperty("Orders")]
        public ShipmentOrder[] Orders { get; set; }
    }
}
