using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models
{
    public class ProductError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("last_occurrence")]
        public DateTime LastOccurrence { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }

    public class ProductErrorCategory
    {
        [JsonProperty("errors")]
        public IList<ProductError> Errors { get; set; }

        [JsonProperty("error_category_code")]
        public string ProductErrorCategoryCode { get; set; }
    }

    public class ProductErrors
    {
        [JsonProperty("entity_id")]
        public string entity_id { get; set; }

        [JsonProperty("categories")]
        public IList<ProductErrorCategory> ProductErrorCategories { get; set; }
    }

    public class B2WErrors
    {
        [JsonProperty("errors_qty")]
        public int ErrorsCount { get; set; }

        [JsonProperty("data")]
        public IList<ProductErrors> ProductErrors { get; set; }
    }
}
