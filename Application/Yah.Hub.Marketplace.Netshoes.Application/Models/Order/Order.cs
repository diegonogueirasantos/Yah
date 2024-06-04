using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Order
    {
        [JsonProperty("orderNumber")] 
        public string IntegrationOrderNumber { get; set; }

        [JsonProperty("agreedDate")] 
        public long AgreedDate { get; set; }

        [JsonProperty("paymentDate")] 
        public long PaymentDate { get; set; }

        [JsonProperty("orderDate")] 
        public long OrderDate { get; set; }

        [JsonProperty("orderType")] 
        public string OrderType { get; set; }

        [JsonProperty("orderStatus")] 
        public string OrderStatus { get; set; }

        [JsonProperty("originSite")]
        public string OriginSite { get; set; }

        [JsonProperty("originNumber")] 
        public string OriginNumber { get; set; }

        [JsonProperty("totalQuantity")] 
        public int TotalQuantity { get; set; }

        [JsonProperty("businessUnit")] 
        public string BusinessUnit { get; set; }

        [JsonProperty("totalGross")] 
        public decimal TotalGross { get; set; }

        [JsonProperty("totalDiscount")] 
        public decimal TotalDiscount { get; set; }

        [JsonProperty("totalFreight")] 
        public decimal TotalFreight { get; set; }

        [JsonProperty("totalNet")] 
        public decimal TotalNet { get; set; }

        [JsonProperty("shippings")] 
        public Shipping[] Shippings { get; set; } = new Shipping[0];
    }

}
