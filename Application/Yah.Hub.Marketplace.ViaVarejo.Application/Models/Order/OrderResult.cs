using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class OrderResult
    {
        public OrderResult()
        {
            Orders = new Order[0];
            Metadata = new OrderMetadata[0];
        }

        [JsonProperty("orders")]
        public Order[] Orders { get; set; }
        [JsonProperty("metadata")]
        public OrderMetadata[] Metadata { get; set; }
    }
}
