using System;
namespace Yah.Hub.Domain.Category
{
    public class MarketplaceCategoryAttribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsMandatory { get; set; }
        public bool AllowVariations { get; set; }
        public List<MarketplaceCategoryAttributeValue> Values { get; set; } = new List<MarketplaceCategoryAttributeValue>();
        public List<MarketplaceCategoryAttributeValue> AllowedUnits { get; set; }
        public string Tooltip { get; set; }
        public string Hint { get; set; }

    }

    public class MarketplaceCategoryAttributeValue
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

