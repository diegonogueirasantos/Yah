using Newtonsoft.Json;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.Magalu.Application.Models;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Sku
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Height")]
        public decimal? Height { get; set; }

        [JsonProperty("Width")]
        public decimal? Width { get; set; }

        [JsonProperty("Length")]
        public decimal? Length { get; set; }

        [JsonProperty("Weight")]
        public decimal? Weight { get; set; }

        [JsonProperty("Status")]
        public bool? Status { get; set; }

        [JsonProperty("Variation")]
        public string Variation { get; set; }

        [JsonProperty("IdSku")]
        public string IdSku { get; set; }

        [JsonProperty("IdSkuErp")]
        public string IdSkuErp { get; set; }

        [JsonProperty("IdProduct")]
        public string IdProduct { get; set; }

        [JsonProperty("CodeEan")]
        public string CodeEan { get; set; }

        [JsonProperty("CodeNcm")]
        public string CodeNcm { get; set; }

        [JsonProperty("CodeIsbn")]
        public string CodeIsbn { get; set; }

        [JsonProperty("CodeNbm")]
        public string CodeNbm { get; set; }

        [JsonProperty("Price")]
        public Price Price { get; set; }

        [JsonProperty("StockQuantity")]
        public long StockQuantity { get; set; }

        [JsonProperty("MainImageUrl")]
        public string MainImageUrl { get; set; }

        [JsonProperty("UrlImages")]
        public List<string> UrlImages { get; set; }

        [JsonProperty("Attributes")]
        public List<Attribute> Attributes { get; set; }
    }
}
