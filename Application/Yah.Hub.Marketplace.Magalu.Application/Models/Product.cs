using Newtonsoft.Json;
using Yah.Hub.Marketplace.Magalu.Application.Models;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Product
    {
        [JsonProperty("IdProduct")]
        public string IdProduct { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("NbmOrigin")]
        public string NbmOrigin { get; set; }

        [JsonProperty("NbmNumber")]
        public string NbmNumber { get; set; }

        [JsonProperty("WarrantyTime")]
        public string WarrantyTime { get; set; }

        [JsonProperty("Active")]
        public bool? Active { get; set; }

        [JsonProperty("Categories")]
        public List<Category> Categories { get; set; }

        [JsonProperty("MarketplaceStructures")]
        public List<Category> MarketplaceStructures { get; set; }

        [JsonProperty("Attributes")]
        public List<Attribute>? Attributes { get; set; }
    }
}
