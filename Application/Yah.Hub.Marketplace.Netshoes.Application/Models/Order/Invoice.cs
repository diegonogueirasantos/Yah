using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Invoice
    {
        [JsonProperty("accessKey")]
        public string Key { get; set; }

        [JsonProperty("shipDate")]
        public long ShipDate { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }
    }
}
