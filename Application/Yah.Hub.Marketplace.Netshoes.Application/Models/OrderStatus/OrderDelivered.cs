using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus
{
    public class OrderDelivered
    {
        [JsonProperty("deliveryDate")]
        public DateTime? DeliveryDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
