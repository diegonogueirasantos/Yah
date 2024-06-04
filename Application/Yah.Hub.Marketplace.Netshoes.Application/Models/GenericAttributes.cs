using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class GenericAttributes
    {
        [JsonProperty("items")]
        public GenericAttribute[] Attributes { get; set; }
    }

    public class GenericAttribute
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
