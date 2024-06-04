using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class Attribute
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Values { get; set; }
    }

    public class Image
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }

    public class Product
    {
        [JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
        public string Sku { get; set; }

        [JsonProperty("productGroup", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductGroup { get; set; }

        [JsonProperty("department", NullValueHandling = NullValueHandling.Ignore)]
        public string Department { get; set; }//todo map

        [JsonProperty("productType", NullValueHandling = NullValueHandling.Ignore)]
        public string ProductType { get; set; }//todo map

        [JsonProperty("brand", NullValueHandling = NullValueHandling.Ignore)]
        public string Brand { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        [JsonProperty("flavor", NullValueHandling = NullValueHandling.Ignore)]
        public string Flavor { get; set; }

        [JsonProperty("gender", NullValueHandling = NullValueHandling.Ignore)]
        public string Gender { get; set; }

        [JsonProperty("manufacturerCode", NullValueHandling = NullValueHandling.Ignore)]
        public string ManufacturerCode { get; set; }//todo map

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("eanIsbn", NullValueHandling = NullValueHandling.Ignore)]
        public string EanIsbn { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public int Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public int Width { get; set; }

        [JsonProperty("depth", NullValueHandling = NullValueHandling.Ignore)]
        public int Depth { get; set; }

        [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
        public int Weight { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public string Video { get; set; }

        [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
        public List<Image> Images { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public ProductStatus Status { get; set; } 


        [JsonIgnore]
        public Stock Stock { get; set; }

        [JsonIgnore]
        public Price Price { get; set; }
    }
}
