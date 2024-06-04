using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class Stock
    {
        [JsonProperty("available")]
        public int Available { get; set; }
    }
}
