using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class OrderResult
    {
        [JsonProperty("items")] 
        public Order[] Items { get; set; } = new Order[0];

        [JsonProperty("page")] 
        public int Page { get; set; }

        [JsonProperty("totalPages")] 
        public int TotalPages { get; set; }

        [JsonProperty("size")] 
        public int Size { get; set; }

        [JsonProperty("total")] 
        public int Total { get; set; }
    }
}
