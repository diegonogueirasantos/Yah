using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Category
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("ParentId")]
        public string ParentId { get; set; }
    }
}
