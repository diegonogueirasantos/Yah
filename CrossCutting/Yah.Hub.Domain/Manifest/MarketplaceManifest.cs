using System.Dynamic;

namespace Yah.Hub.Domain.Manifest
{
    public class MarketplaceManifest
    {
        public IEnumerable<Field> Fields { get; set; }
    }

    public class Field
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string FieldType { get; set; }
        public FieldLocation FieldLocation { get; set; }
        public IList<Validations> Validations { get; set; }
        public bool IsRequired { get; set; }

    }

    public class Validations
    {
        public string ValidationType { get; set; }
        public string DisplayName { get; set; }
        public ExpandoObject Params { get; set; }
    }

    [Serializable]
    public enum FieldLocation
    {
        Product,
        Sku
    }
}
