using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Infractions
{
    public class MercadoLivreInfractions
    {
        [JsonProperty("infractions")]
        public List<Infraction> Infractions { get; set; }
    }

    public class Infraction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("related_item_id")]
        public string RelatedItemId { get; set; }

        [JsonProperty("element_id")]
        public string ElementId { get; set; }

        [JsonProperty("element_type")]
        public string ElementType { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("filter_subgroup")]
        public string FilterGroup { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("remedy")]
        public string Remedy { get; set; }

        [JsonProperty("suggested")]
        public Suggested Suggested { get; set; }
    }

    public class Suggested
    {
        [JsonProperty("categories")]
        public List<SuggestCategory> SuggestCategory { get; set; }
    }

    public class SuggestCategory
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
