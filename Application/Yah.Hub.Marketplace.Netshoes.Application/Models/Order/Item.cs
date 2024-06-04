using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Item
    {
        [JsonProperty("itemId")]
        public int Id { get; set; }

        [JsonProperty("manufacturerCode")]
        public string ManufacturerCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("sku")]
        public string SKU { get; set; }

        [JsonProperty("departmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("departmentCode")]
        public int DepartmentCode { get; set; }

        [JsonProperty("totalGross")]
        public decimal TotalGross { get; set; }

        [JsonProperty("totalDiscount")]
        public decimal TotalDiscount { get; set; }

        [JsonProperty("totalFreight")]
        public decimal TotalFreight { get; set; }

        [JsonProperty("totalNet")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("grossUnitValue")]
        public decimal GrossUnitValue { get; set; }

        [JsonProperty("discountUnitValue")]
        public decimal DiscountUnitValue { get; set; }

        [JsonProperty("netUnitValue")]
        public decimal NetUnitValue { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("flavor")]
        public string Flavor { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
