using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Transport
    {
        [JsonProperty("trackingNumber")] 
        public string TrackingNumber { get; set; }

        [JsonProperty("trackingLink")] 
        public string TrackingLink { get; set; }

        [JsonProperty("trackingShipDate")] 
        public long TrackingShipDate { get; set; }

        [JsonProperty("deliveryDate")] 
        public long DeliveryDate { get; set; }

        [JsonProperty("shipDate")] 
        public long ShipDate { get; set; }

        [JsonProperty("deliveryService")] 
        public string DeliveryService { get; set; }

        [JsonProperty("carrier")] 
        public string Carrier { get; set; }

        [JsonProperty("CarrierId")]
        public string CarrierId { get; set; }
    }
}
