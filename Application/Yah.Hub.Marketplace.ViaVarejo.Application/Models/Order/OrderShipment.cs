using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class OrderShipment
    {
        [JsonProperty("items")]
        public string[] Items { get; set; }

        [JsonProperty("occurredAt")]
        public string OccurredAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("number")]
        public string TrackingCode { get; set; }

        [JsonProperty("sellerDeliveryId")]
        public string SellerDeliveryId { get; set; }

        [JsonProperty("cte")]
        public string Cte { get; set; }

        [JsonProperty("carrier")]
        public Carrier Carrier { get; set; }
    }
}
