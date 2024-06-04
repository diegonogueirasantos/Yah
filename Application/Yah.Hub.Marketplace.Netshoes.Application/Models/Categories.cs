using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class CategoriesTree
    {
        [JsonProperty("items")]
        public Category[] Categories { get; set; }
    }

    public class Category
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }


}
