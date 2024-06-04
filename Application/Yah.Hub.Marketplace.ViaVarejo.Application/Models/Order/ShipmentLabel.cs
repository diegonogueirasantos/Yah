using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class ShipmentLabel
    {
        [JsonProperty("labels")]
        public Label[] Labels { get; set; }
    }

    public class Label
    {
        [JsonProperty("skuSellerId")]
        public string SkuSellerId { get; set; }

        [JsonProperty("deliveryId")]
        public long DeliveryId { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("pdf")]
        public string Pdf { get; set; }

        [JsonProperty("zpl")]
        public string Zpl { get; set; }

        [JsonProperty("validity")]
        public Validity Validity { get; set; }
    }

    public class Validity
    {
        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }
}
