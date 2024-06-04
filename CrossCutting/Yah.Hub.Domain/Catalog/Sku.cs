using System.Dynamic;

namespace Yah.Hub.Domain.Catalog
{
    public class Sku
    {
        public string ProductId { get; set; }

        /// <summary>
        /// SKU Identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// External Indentifier
        /// </summary>
        public string IntegrationId { get; set; }

        /// <summary>
        /// Name of Sku
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SKU specific pricing information
        /// </summary>
        public Price Price { get; set; }

        /// <summary>
        /// SKU specific inventory information
        /// </summary>
        public Inventory Inventory { get; set; }

        public Dimension Dimension { get; set; }

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
        public ExpandoObject? SkuAttributes { get; set; }

        public List<Variation>? Variations { get; set; }

        public List<Image> Images { get; set; }


    }

    public class Variation
    {
        /// <summary>
        /// Variation Identifier
        /// </summary>
        /// <example>
        /// "Color, Size, Flavor"
        /// </example>
        public string Id { get; set; }

        /// <summary>
        /// Value of variation
        /// </summary>
        /// <example>
        /// "12345"
        /// </example>
        public string ValueId { get; set; }

        /// <summary>
        /// Name of value
        /// </summary>
        /// <example>
        /// "Black, White, Blue, Pink"
        /// </example>
        public string Value { get; set; }
    }

    public class Dimension
    {
        public string Height { get; set; }
        public string Width { get; set; }
        public string Length { get; set; }
        public string Weight { get; set; }
    }
}
