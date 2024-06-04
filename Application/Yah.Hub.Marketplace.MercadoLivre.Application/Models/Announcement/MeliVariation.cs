using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliVariation
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("attribute_combinations")]
        public List<MeliAttribute> AttributesCombinations { get; set; } = new List<MeliAttribute>();

        [JsonProperty("attributes")]
        public List<MeliAttribute> Attributes { get; set; } = new List<MeliAttribute>();

        [JsonProperty("seller_custom_field")]
        public string SellerCustomField { get; set; }

        [JsonProperty("available_quantity", DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? Balance { get; set; }

        [JsonProperty("sold_quantity")]
        public int? Sold { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("picture_ids", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Pictures { get; set; } = new List<string>();
    }
}
