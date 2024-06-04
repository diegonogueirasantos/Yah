using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliAnnoucementPrice
    {
        [JsonIgnore]
        public string ItemId { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("variations")]
        public List<AnnoucementPriceVariation> Variations { get; set; }
    }

    public class AnnoucementPriceVariation
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
