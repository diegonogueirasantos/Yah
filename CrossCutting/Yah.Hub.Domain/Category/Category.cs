using System;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Identity;

namespace Yah.Hub.Domain.Category
{
    public class MarketplaceCategory : BaseEntity
    {
        public MarketplaceCategory(string id)
        {
            this.Id = id;
        }

        public string ParentId { get; set; }
        public string Name { get; set; }
        public List<MarketplaceCategoryPath> Path { get; set; }
        public bool  HasChildren { get; set; }
        public List<MarketplaceCategoryAttribute> Attributes { get; set; } = new List<MarketplaceCategoryAttribute>();
        public List<MarketplaceChildrenCategory> Childrens { get; set; } = new List<MarketplaceChildrenCategory>();
    }

    public class MarketplaceChildrenCategory
    {
        public MarketplaceChildrenCategory() { }
        public MarketplaceChildrenCategory(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class MarketplaceCategoryPath
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

}

