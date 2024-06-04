using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models.Order
{
    public class OrderShipment
    {
        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("customer")]
        public string customer { get; set; }

        [JsonProperty("value")]
        public double value { get; set; }
    }

    public class Plp
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("expiration_date")]
        public string expiration_date { get; set; }

        [JsonProperty("printed")]
        public bool printed { get; set; }

        [JsonProperty("type")]
        public object type { get; set; }

        [JsonProperty("orders")]
        public List<OrderShipment> orders { get; set; }
    }

    public class PlpGroup
    {
        [JsonProperty("order_remote_codes")]
        public List<string> OrderIds { get; set; }
    }


    public class Shipment
    {
        [JsonProperty("plp")]
        public List<Plp> plps { get; set; }
    }

    public class UngruoupPlpRequest
    {
        [JsonProperty("plp_id")]
        public string PlpId { get; set; }
    }

    public class ConfirmCollection
    {
        [JsonProperty("order_codes")]
        public List<string> order_codes { get; set; }
    }

}

