using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class ProductWrapper
    {
        public ProductWrapper(Product[] items)
        {
            Items = items;
        }

        [JsonProperty("itens")]
        public Product[] Items { get; set; }
    }
}
