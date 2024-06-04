using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliAttribute
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value_id", NullValueHandling = NullValueHandling.Include)]
        public string Value { get; set; }

        [JsonProperty("value_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ValueName { get; set; }
    }
}
