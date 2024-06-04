using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models.Order
{
    public class B2WItem
    {
        [JsonProperty("sku")]
        public string sku { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }
    }

    public class B2WShipOrder : B2WOrderStatus
    {
        [JsonProperty("shipment")]
        public B2WShipment shipment { get; set; }
    }

    public class B2WShipment
    {
        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("delivered_carrier_date")]
        public DateTime delivered_carrier_date { get; set; }

        [JsonProperty("items")]
        public List<B2WItem> items { get; set; }

        [JsonProperty("track")]
        public B2WTrack track { get; set; }
    }

    public class B2WTrack
    {
        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("carrier")]
        public string carrier { get; set; }

        [JsonProperty("method")]
        public string method { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }
    }
}

