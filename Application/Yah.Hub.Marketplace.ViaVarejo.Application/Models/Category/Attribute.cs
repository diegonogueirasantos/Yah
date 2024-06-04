using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Category
{
    public class Attribute
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("required")]
        public string Required { get; set; }

        [JsonProperty("variant")]
        public string Variant { get; set; }

        [JsonProperty("values")]
        public Value[] Value { get; set; }
    }

    public class Value
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
