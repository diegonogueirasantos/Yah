using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class Price
    {
        [JsonProperty("listPrice")]
        public decimal ListPrice { get; set; }

        [JsonProperty("salePrice")]
        public decimal SalePrice { get; set; }
    }
}
