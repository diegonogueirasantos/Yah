using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales
{
    public class OrderClientResult
    {
        [JsonProperty("results")]
        public List<MeliOrder> Results { get; set; }

        [JsonProperty("paging")]
        public OrderListPaging Paging { get; set; }
    }
    public class OrderListPaging
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
}
