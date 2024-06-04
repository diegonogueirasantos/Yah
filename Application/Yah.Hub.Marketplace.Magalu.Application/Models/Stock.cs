using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Stock
    {
        [JsonProperty("IdSku")]
        public string IdSku { get; set; }

        [JsonProperty("Quantity")]
        public long? Quantity { get; set; }
    }
}
