using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class OrderQueueItemId
    {
        public OrderQueueItemId(int id) => Id = id;

        [JsonProperty("Id")]
        public int Id { get; set; }
    }
}
