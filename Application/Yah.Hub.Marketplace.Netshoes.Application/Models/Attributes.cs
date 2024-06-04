using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class AttributeGroup
    {
        [JsonProperty("items")]
        public CategoryAttributes[] Attributes { get; set; }
    }

    public class CategoryAttributes
    {
        [JsonProperty("code")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("typeSelection")]
        public string TypeSelection { get; set; }

        [JsonProperty("required")]
        public bool IsRequired { get; set; }

        [JsonProperty("values")]
        public AttributeValue[] Values { get; set; }
    }


    public class AttributeValue
    {
        [JsonProperty("code")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
