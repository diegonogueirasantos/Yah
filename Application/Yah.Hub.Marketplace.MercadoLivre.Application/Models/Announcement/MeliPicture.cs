using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliPicture
    {
        public MeliPicture() { }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("secure_url")]
        public string SecureUrl { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("max_size")]
        public string MaxSize { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

    }
}
