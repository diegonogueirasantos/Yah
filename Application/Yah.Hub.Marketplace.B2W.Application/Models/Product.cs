using Newtonsoft.Json;
using Yah.Hub.Common.Enums;
using Yah.Hub.Marketplace.B2W.Application.Models;

namespace Yah.Hub.Marketplace.B2W.Application.Models
{
    public class ProductWrapper
    {
        [JsonProperty("product")]
        public Product Product { get; set; }
    }

    public class Product
    {
        public Product()
        {
            this.Status = "enabled";
        }

        #region Basic Info
        [JsonProperty("sku", Required = Required.Always)]
        public string Sku { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("qty", NullValueHandling = NullValueHandling.Ignore)]
        public int? Inventory { get; set; }

        [JsonProperty("crossDocking", NullValueHandling = NullValueHandling.Ignore)]
        public int? CrossDocking { get; set; }

        #endregion

        #region Prices
        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Price { get; set; }

        [JsonProperty("promotional_price", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? PromotionalPrice { get; set; }

        [JsonProperty("cost", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Cost { get; set; }
        #endregion

        #region Dimensions

        [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Weight { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Width { get; set; }

        [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Length { get; set; }

        #endregion

        #region Additional Info
        [JsonProperty("brand", NullValueHandling = NullValueHandling.Ignore)]
        public string Brand { get; set; }

        [JsonProperty("nbm", NullValueHandling = NullValueHandling.Ignore)]
        public string NBM { get; set; }

        [JsonProperty("ean", NullValueHandling = NullValueHandling.Ignore)]
        public string EAN { get; set; }
        #endregion

        #region Categories
        [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
        public List<Category> Categories { get; set; }
        #endregion

        #region Images
        [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ImagesUrls { get; set; }
        #endregion

        #region Specifications
        [JsonProperty("specifications", NullValueHandling = NullValueHandling.Ignore)]
        public List<Specification> Specifications { get; set; }
        #endregion

        #region Variations
        [JsonProperty("variations", NullValueHandling = NullValueHandling.Ignore)]
        public List<Variation>? Variations { get; set; }

        [JsonProperty("variation_attributes", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> VariationAttributes { get; set; }
        #endregion

        #region Associations

        [JsonProperty("associations", NullValueHandling = NullValueHandling.Ignore)]
        public List<B2WAssociation> Associations { get; set; }

        #endregion
    }

    public class Category
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class Specification
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class B2WAssociation
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public EntityStatus GetProductStatus(bool hasErrors)
        {
            var currentStatus = Status.ToLowerInvariant();

            switch (currentStatus)
            {
                case "linked":
                    return EntityStatus.Accepted;
                case "declined":
                    return EntityStatus.Declined;
                case "waiting_confirmation":
                    return EntityStatus.PendingValidation;
                default:
                    return hasErrors ? EntityStatus.Declined : EntityStatus.Sent;
            }
        }
    }
}
