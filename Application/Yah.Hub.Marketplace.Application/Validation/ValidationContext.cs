using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Manifest;

namespace Yah.Hub.Marketplace.Application.Validation
{
    public class ValidationContext
    {
        public string Marketaplace { get; set; }
        public object Product { get; set; }
        public Field Field { get; set; }
        public string Sku { get; set; }
    }
}
