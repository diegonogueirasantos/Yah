using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class ShipmentLabelResult
    {
        [JsonProperty("trackingGroupNumber")]
        public string TrackingGroupNumber { get; set; }

        [JsonProperty("trackingGroupStatus")]
        public string TrackingGroupStatus { get; set; }

        [JsonProperty("labelStatus")]
        public string LabelStatus { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("tag")]
        public Tag Tag { get; set; }

        [JsonProperty("trackings")]
        public List<Tracking> Trackings { get; set; }
    }

    public class Tag
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Tracking
    {
        [JsonProperty("shippingCode")]
        public int ShippingCode { get; set; }

        [JsonProperty("trackingCode")]
        public string TrackingCode { get; set; }

        [JsonProperty("trackingStatus")]
        public string TrackingStatus { get; set; }

        [JsonProperty("labelStatus")]
        public string LabelStatus { get; set; }

        [JsonProperty("trackingLink")]
        public string TrackingLink { get; set; }
    }


}
