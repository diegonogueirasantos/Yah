using System;
using System.Data;
using Newtonsoft.Json;
using Yah.Hub.Common.Enums;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Domain.Announcement
{
    public class SaleTerm
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value_name")]
        public string ValueName { get; set; }
    }

    public class Picture
    {
        [JsonProperty("source")]
        public string Source { get; set; }
    }

    public class Attribute
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value_id", NullValueHandling = NullValueHandling.Include)]
        public string Value { get; set; }

        [JsonProperty("value_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ValueName { get; set; }
    }

    public class AnnouncementItem
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("price")]
        public float Price { get; set; }

        [JsonProperty("status")]
        public EntityStatus Status { get; set; }

        public string[] SubStatus { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("available_quantity")]
        public int AvailableQuantity { get; set; }

        [JsonProperty("buying_mode")]
        public string BuyingMode { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("listing_type_id")]
        public string ListingTypeId { get; set; }

        [JsonProperty("sale_terms")]
        public List<Attribute> SaleTerms { get; set; } = new List<Attribute>();

        [JsonProperty("pictures")]
        public List<Picture> Pictures { get; set; }

        [JsonProperty("attributes")]
        public List<Attribute> Attributes { get; set; }

        public Dimension Dimension { get; set; }

        public Shipping Shipping { get; set; }

        public List<AnnouncementVariation>? Variations { get; set; }
    }

    public class Shipping
    {
        public string ShippingMode { get; set; }
        public bool FreeShipping { get; set; }
        public string LogisticType { get; set; }
        public string[] Tags { get; set; }
        public FreeMethods? FreeMethods { get; set; }
    }

    public class FreeMethods
    {
        public int Id { get; set; }

        public Rules Rule { get; set; } = new Rules();
    }

    public class Rules
    {
        [JsonProperty("default")]
        public bool Default { get; set; }

        [JsonProperty("free_mode")]
        public string FreeMode { get; set; }

        [JsonProperty("free_shipping_flag")]
        public bool FreeShippingFlag { get; set; }

        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }

    public class AnnouncementVariation
    {
        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("attribute_combinations")]
        public List<Attribute> AttributesCombinations { get; set; } = new List<Attribute>();

        [JsonProperty("attributes")]
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        [JsonProperty("seller_custom_field")]
        public string SellerCustomField { get; set; }

        [JsonProperty("available_quantity", DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? Balance { get; set; }

        [JsonProperty("sold_quantity")]
        public int? Sold { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("picture_ids", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Pictures { get; set; } = new List<string>();
    }
}

