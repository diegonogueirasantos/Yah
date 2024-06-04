using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models
{
    public class ProductStatus
    {
        [JsonProperty("reviews")]
        public ProductReview[] Reviews { get; set; }

        [JsonProperty("status")]
        public ProductStatusEnum? Status { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }

    public class ProductReview
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public enum ProductStatusEnum
    {
        RECEIVED,
        CATALOGING,
        CRITICIZED,
        APPROVED,
        READY_TO_SALE
    }
}
