using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class InventoryUpdate
    {
        [JsonProperty("idSkuLojista")]
        public string SkuId { get; set; }

        [JsonProperty("estoque")]
        public Inventory Inventory { get; set; }
    }
}
