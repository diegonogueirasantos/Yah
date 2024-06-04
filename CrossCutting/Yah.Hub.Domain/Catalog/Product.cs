using Newtonsoft.Json;
using Yah.Hub.Common.Enums;
using System.Dynamic;

namespace Yah.Hub.Domain.Catalog
{
    public class Product
    {
        public List<Sku> Skus { get; set; }

        /// <summary>
        /// Product Identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// App and Marketplace Integration Identifier
        /// </summary>
        public string IntegrationId { get; set; }

        /// <summary>
        /// Product description information
        /// </summary>
        public string Description { get; set; }

        public string Brand { get; set; }

        public int WarrantyTime { get; set; }

        public WarrantyType WarrantyType { get; set; }

        /// <summary>
        /// Product Attributes receved for Integration
        /// </summary>
        /// <example>
        /// Ex.: 
        /// {
        ///   "ean": "1234567891234",
        ///   "brand": "Adidas",
        ///   "nbm": "897887987"
        /// }
        /// </example>
        public ExpandoObject? Attributes { get; set; }

        public List<Image> Images { get; set; }

        public Category Category { get; set; }

        [JsonIgnore]
        public bool HasVariations
        {
            get
            {
                return this.Skus != null && this.Skus.Any() && this.Skus.First().Variations != null && this.Skus.First().Variations.Any();
            }
        }
    }
}
