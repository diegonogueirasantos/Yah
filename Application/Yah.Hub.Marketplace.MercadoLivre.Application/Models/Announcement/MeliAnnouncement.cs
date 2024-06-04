using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliAnnouncement
    {
        [JsonProperty("id")]
        public string ItemId { get; set; }

        [JsonProperty("listing_type_id")]
        public string ListingType { get; set; }

        [JsonProperty("available_quantity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Balance { get; set; }

        [JsonProperty("sold_quantity")]
        public int? Sold { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("status")]
        public AnnouncementStatus Status { get; set; }

        [JsonProperty("description")]
        public Description Description { get; set; }

        [JsonProperty("sub_status")]
        public string[] SubStatus { get; set; }
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("currency_id")]
        public string Currency { get => "BRL"; }

        [JsonProperty("buying_mode")]
        public string BuyingMode { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string Subtitle { get; set; }

        [JsonProperty("seller_custom_field")]
        public string SellerCustomField { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("domain_id")]
        public string DomainId { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("video_id")]
        public string VideoId { get; set; }

        [JsonProperty("permalink")]
        public string PermaLink { get; set; }

        [JsonProperty("channels")]
        public List<string> Channels { get; set; }

        [JsonProperty("shipping")]
        public MeliShipping Shipping { get; set; }

        [JsonProperty("seller_address")]
        public MeliAddress SellerAddress { get; set; }

        [JsonProperty("variations")]
        public List<MeliVariation> Variations { get; set; } = new List<MeliVariation>();

        [JsonProperty("pictures", NullValueHandling = NullValueHandling.Ignore)]
        public List<MeliPicture> Picture { get; set; } = new List<MeliPicture>();

        [JsonProperty("attributes")]
        public List<MeliAttribute> Attributes { get; set; } = new List<MeliAttribute>();

        [JsonProperty("sale_terms")]
        public List<MeliAttribute> SaleTerms { get; set; } = new List<MeliAttribute>();
    }

    public enum AnnouncementStatus
    {
        [EnumMember(Value = "active")]
        Active,
        [EnumMember(Value = "paused")]
        Paused,
        [EnumMember(Value = "closed")]
        Closed,
        [EnumMember(Value = "under_review")]
        UnderReview,
        [EnumMember(Value = "inactive")]
        Inactive
    }
}
