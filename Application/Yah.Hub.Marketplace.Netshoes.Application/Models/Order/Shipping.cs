using Newtonsoft.Json;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Enums;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Shipping
    {
        [JsonProperty("shippingCode")] 
        public string Code { get; set; }

        [JsonProperty("status")] 
        public string Status { get; set; }

        [JsonProperty("freightAmount")] 
        public decimal TotalFreight { get; set; }

        [JsonProperty("deliveryTime")] 
        public short DeliveryTime { get; set; }

        [JsonProperty("country")] 
        public string Country { get; set; }

        [JsonProperty("transport")] 
        public Transport Transport { get; set; }

        [JsonProperty("invoice")] 
        public Invoice Invoice { get; set; }

        [JsonProperty("customer")] 
        public Customer Customer { get; set; }

        [JsonProperty("sender")] 
        public Sender Sender { get; set; }

        [JsonProperty("items")] 
        public Item[] Items { get; set; } = new Item[0];

        [JsonProperty("platformId")]
        public QuotationOrigin PlatformId { get; set; }
    }
}
