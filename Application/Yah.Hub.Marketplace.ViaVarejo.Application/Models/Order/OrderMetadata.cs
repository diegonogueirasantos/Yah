using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class OrderMetadata
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
