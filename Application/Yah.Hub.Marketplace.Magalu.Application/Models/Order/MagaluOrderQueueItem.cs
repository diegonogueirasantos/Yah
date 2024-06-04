using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluOrderQueueItem
    {
        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("IdOrder")]
        public string IdOrder { get; set; }

        [JsonProperty("IdOrderMarketplace")]
        public string IdOrderMarketplace { get; set; }

        [JsonProperty("InsertedDate")]
        public DateTimeOffset? InsertedDate { get; set; }

        [JsonProperty("OrderStatus")]
        public string OrderStatus { get; set; }
    }
}
