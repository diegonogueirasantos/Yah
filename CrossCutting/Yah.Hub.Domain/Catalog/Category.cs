namespace Yah.Hub.Domain.Catalog
{
    public class Category
    {
        public int Id { get; set; }

        public List<CategoryPath> Path { get; set; }

        public string Name { get; set; }

        public string MarketplaceId { get; set; }

        public int? ParentId { get; set; }
    }

    public class CategoryPath
    {
        public int ParentId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
