using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus
{
    public class OrderCanceled
    {
        [JsonProperty("cancellationReason")]
        public string ReasonCancellationCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
