using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Sender
    {
        [JsonProperty("supplierCnpj")]
        public string SupplierCnpj { get; set; }

        [JsonProperty("sellerCode")]
        public string SellerCode { get; set; }

        [JsonProperty("sellerName")]
        public string SellerName { get; set; }

        [JsonProperty("supplierName")]
        public string SupplierName { get; set; }
    }
}
