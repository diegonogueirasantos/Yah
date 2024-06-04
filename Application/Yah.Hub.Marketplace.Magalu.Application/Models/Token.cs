using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Token
    {
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        [JsonProperty("redirect_uri")]
        public string RedirectUri { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("refresh_token", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken { get; set; }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }
    }
}
