using Newtonsoft.Json;

namespace Yah.Hub.Domain.Catalog
{
    public class ProductInventory
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string IntegrationId { get; set; }

        public SkuInventory AffectedSku { get; set; }
        
        [JsonIgnore]
        public Announcement.Announcement Announcement { get; set; }

        public List<Sku> Skus { get; set; }

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
