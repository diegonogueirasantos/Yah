using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class ShipmentLabelRequest
    {
        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("shippingCodes")]
        public string[] ShippingCodes { get; set; }
    }
}
