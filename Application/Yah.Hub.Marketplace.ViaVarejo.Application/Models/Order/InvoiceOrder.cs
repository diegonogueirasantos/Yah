using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class InvoiceOrder
    {
        [JsonProperty("items")]
        public string[] Items { get; set; }

        [JsonProperty("occurredAt")]
        public string OccurredAt { get; set; }

        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }
    }
}
