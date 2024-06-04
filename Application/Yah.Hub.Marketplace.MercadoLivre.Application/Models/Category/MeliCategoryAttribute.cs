using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Category
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class MeliCategoryAttribute
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("tags")]
        public Tags tags { get; set; }

        [JsonProperty("hierarchy")]
        public string hierarchy { get; set; }

        [JsonProperty("relevance")]
        public int relevance { get; set; }

        [JsonProperty("value_type")]
        public string value_type { get; set; }

        [JsonProperty("allowed_units")]
        public List<Value> allowed_units { get; set; }

        [JsonProperty("values")]
        public List<Value> values { get; set; }

        [JsonProperty("value_max_length")]
        public int value_max_length { get; set; }

        [JsonProperty("attribute_group_id")]
        public string attribute_group_id { get; set; }

        [JsonProperty("attribute_group_name")]
        public string attribute_group_name { get; set; }

        [JsonProperty("hint")]
        public string hint { get; set; }

        [JsonProperty("tooltip")]
        public string tooltip { get; set; }
    }

    public class Tags
    {
        [JsonProperty("catalog_required")]
        public bool catalog_required { get; set; }

        [JsonProperty("required")]
        public bool required { get; set; }

        [JsonProperty("variation_attribute")]
        public bool variation_attribute { get; set; }
    }

    public class Value
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("metadata")]
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("value")]
        public bool value { get; set; }
    }
}

