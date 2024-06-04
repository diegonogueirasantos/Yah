using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class ShipmentLabelRequest
    {
        public ShipmentLabelRequest(string orderId)
        {
            OrderId = orderId;
        }

        [JsonProperty("labelsNumber")]
        public int LabelsNumber { get; set; } = 1;

        [JsonProperty("link")]
        public bool Link { get; set; } = true;

        [JsonProperty("concat")]
        public bool Concat { get; set; } = true;

        [JsonIgnore]
        public string OrderId { get; set; }
    }
}
