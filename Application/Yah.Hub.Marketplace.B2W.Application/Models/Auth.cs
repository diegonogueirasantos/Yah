using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models
{
	public class Auth
	{
        [JsonProperty("user_email", Required = Required.Always)]
        public string email { get; set; }

        [JsonProperty("api_key", NullValueHandling = NullValueHandling.Ignore)]
        public string accessKey { get; set; }
    }

    public class AuthResponse
    {
        [JsonProperty("token", Required = Required.Always)]
        public string token { get; set; }
    }
}