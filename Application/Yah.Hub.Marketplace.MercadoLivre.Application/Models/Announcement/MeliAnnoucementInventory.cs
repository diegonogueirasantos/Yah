using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliAnnoucementInventory
    {
        [JsonIgnore]
        public string ItemId { get; set; }

        [JsonProperty("available_quantity")]
        public int? Stock { get; set; }

        [JsonProperty("variations")]
        public List<AnnoucementInventoryVariation> Variations { get; set; }
    }

    public class AnnoucementInventoryVariation
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("available_quantity")]
        public int Stock { get; set; }
    }
}
