using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluOrderQueue
    {
        [JsonProperty("Total")]
        public int? Total { get; set; }

        [JsonProperty("OrderQueues")]
        public List<MagaluOrderQueueItem> Orders { get; set; }
    }
}
