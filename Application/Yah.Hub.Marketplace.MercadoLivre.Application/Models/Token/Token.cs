using System;
using Nest;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Token
{
    public class Token
    {
        public Token()
        {
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public Int64 ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }

    }
}

