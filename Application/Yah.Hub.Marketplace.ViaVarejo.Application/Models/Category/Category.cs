using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Category
{
    public class CategoryTree
    {
        [JsonProperty("categories")]
        public Category[] Categories { get; set; }
    }

    public class Category
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        [JsonProperty("categories")]
        public Category[] Categories { get; set; }

        [JsonProperty("groups")]
        public Group[] Groups { get; set; }
    }

    public class Group
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("attributes")]
        public Attribute[] Attributes { get; set; }
    }
}
