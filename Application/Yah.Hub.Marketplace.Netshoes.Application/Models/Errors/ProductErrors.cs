using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Errors
{
    public class ProductErrors
    {
        [JsonProperty("errors")]
        public string[] Errors { get; set; } = new string[0];
    }
}
