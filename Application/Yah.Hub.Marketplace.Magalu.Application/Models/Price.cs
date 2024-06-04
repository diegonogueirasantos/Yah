using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Price
    {
        [JsonProperty("IdSku")]
        public string IdSku { get; set; }

        [JsonProperty("ListPrice")]
        public decimal? ListPrice { get; set; }

        [JsonProperty("SalePrice")]
        public decimal? SalePrice { get; set; }
    }
}
