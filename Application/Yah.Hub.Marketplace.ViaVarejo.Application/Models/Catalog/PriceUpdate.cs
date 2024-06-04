using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class PriceUpdate
    {
        [JsonProperty("idSkuLojista")]
        public string SkuId { get; set; }

        [JsonProperty("preco")]
        public Price Price { get; set; }
    }
}
