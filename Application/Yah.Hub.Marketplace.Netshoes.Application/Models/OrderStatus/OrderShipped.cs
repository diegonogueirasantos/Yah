using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus
{
    public class OrderShipped
    {
        [JsonProperty("carrier")]
        public string Carrier { get; set; }

        [JsonProperty("trackingLink")]
        public string TrackingLink { get; set; }

        [JsonProperty("deliveredCarrierDate")]
        public DateTime? DeliveredCarrierDate { get; set; }

        [JsonProperty("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonProperty("estimatedDelivery")]
        public DateTime? EstimatedDelivery { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
