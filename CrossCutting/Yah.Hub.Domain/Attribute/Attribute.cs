namespace Yah.Hub.Domain.Attribute
{
    public class MarketplaceAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<MarketplaceAttributeOptions> Values { get; set; }
    }

    public class MarketplaceAttributeOptions
    {
        public string Id { get; set; }

        public string Name { get; set; }

    }
}
