using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models
{
    public class BillingAddress
    {
        [JsonProperty("street")]
        public string street { get; set; }

        [JsonProperty("secondary_phone")]
        public string secondary_phone { get; set; }

        [JsonProperty("region")]
        public string region { get; set; }

        [JsonProperty("reference")]
        public string reference { get; set; }

        [JsonProperty("postcode")]
        public string postcode { get; set; }

        [JsonProperty("phone")]
        public string phone { get; set; }

        [JsonProperty("number")]
        public string number { get; set; }

        [JsonProperty("neighborhood")]
        public string neighborhood { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("detail")]
        public string detail { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("complement")]
        public string complement { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }
    }

    public class Customer
    {
        [JsonProperty("vat_number")]
        public string vat_number { get; set; }

        [JsonProperty("phones")]
        public List<string> phones { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("gender")]
        public string gender { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("date_of_birth")]
        public string date_of_birth { get; set; }
    }

    public class ImportInfo
    {
        [JsonProperty("ss_name")]
        public string ss_name { get; set; }

        [JsonProperty("remote_id")]
        public string remote_id { get; set; }

        [JsonProperty("remote_code")]
        public string remote_code { get; set; }
    }

    public class Item
    {
        [JsonProperty("special_price")]
        public decimal special_price { get; set; }

        [JsonProperty("shipping_cost")]
        public decimal? shipping_cost { get; set; }

        [JsonProperty("remote_store_id")]
        public object remote_store_id { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("product_id")]
        public string product_id { get; set; }

        [JsonProperty("original_price")]
        public decimal original_price { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("gift_wrap")]
        public object gift_wrap { get; set; }

        [JsonProperty("detail")]
        public object detail { get; set; }
    }

    public class Payment
    {
        [JsonProperty("value")]
        public double value { get; set; }

        [JsonProperty("status")]
        public object status { get; set; }

        [JsonProperty("sefaz")]
        public Sefaz sefaz { get; set; }

        [JsonProperty("parcels")]
        public int parcels { get; set; }

        [JsonProperty("method")]
        public string method { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("card_issuer")]
        public string card_issuer { get; set; }

        [JsonProperty("autorization_id")]
        public string autorization_id { get; set; }
    }

    public class B2WOrder
    {
        [JsonProperty("updated_at")]
        public DateTime updated_at { get; set; }

        [JsonProperty("total_ordered")]
        public double total_ordered { get; set; }

        [JsonProperty("tags")]
        public List<object> tags { get; set; }

        [JsonProperty("sync_status")]
        public string sync_status { get; set; }

        [JsonProperty("status")]
        public Status status { get; set; }

        [JsonProperty("shipping_method")]
        public string shipping_method { get; set; }

        [JsonProperty("shipping_estimate_id")]
        public string shipping_estimate_id { get; set; }

        [JsonProperty("shipping_cost")]
        public double shipping_cost { get; set; }

        [JsonProperty("shipping_carrier")]
        public string shipping_carrier { get; set; }

        [JsonProperty("shipping_address")]
        public ShippingAddress shipping_address { get; set; }

        [JsonProperty("shipped_date")]
        public string shipped_date { get; set; }

        [JsonProperty("shipments")]
        public List<object> shipments { get; set; }

        [JsonProperty("seller_shipping_cost")]
        public double seller_shipping_cost { get; set; }

        [JsonProperty("shipping_method")]
        public string ShippingMethod { get; set; }

        [JsonProperty("placed_at")]
        public DateTime placed_at { get; set; }

        [JsonProperty("payments")]
        public List<Payment> payments { get; set; }

        [JsonProperty("items")]
        public List<Item> items { get; set; }

        [JsonProperty("invoices")]
        public List<object> invoices { get; set; }

        [JsonProperty("interest")]
        public double interest { get; set; }

        [JsonProperty("imported_at")]
        public DateTime imported_at { get; set; }

        [JsonProperty("import_info")]
        public ImportInfo import_info { get; set; }

        [JsonProperty("exported_at")]
        public DateTime? exported_at { get; set; }

        [JsonProperty("estimated_delivery_shift")]
        public object estimated_delivery_shift { get; set; }

        [JsonProperty("estimated_delivery")]
        public DateTime estimated_delivery { get; set; }

        [JsonProperty("discount")]
        public double discount { get; set; }

        [JsonProperty("delivery_contract_type")]
        public string delivery_contract_type { get; set; }

        [JsonProperty("delivered_date")]
        public object delivered_date { get; set; }

        [JsonProperty("customer")]
        public Customer customer { get; set; }

        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("channel")]
        public string channel { get; set; }

        [JsonProperty("calculation_type")]
        public string calculation_type { get; set; }

        [JsonProperty("billing_address")]
        public BillingAddress billing_address { get; set; }
    }

    public class Sefaz
    {
        [JsonProperty("type_integration")]
        public string type_integration { get; set; }

        [JsonProperty("payment_indicator")]
        public string payment_indicator { get; set; }

        [JsonProperty("name_payment")]
        public string name_payment { get; set; }

        [JsonProperty("name_card_issuer")]
        public string name_card_issuer { get; set; }

        [JsonProperty("id_payment")]
        public string id_payment { get; set; }

        [JsonProperty("id_card_issuer")]
        public string id_card_issuer { get; set; }
    }

    public class ShippingAddress
    {
        [JsonProperty("street")]
        public string street { get; set; }

        [JsonProperty("secondary_phone")]
        public string secondary_phone { get; set; }

        [JsonProperty("region")]
        public string region { get; set; }

        [JsonProperty("reference")]
        public string reference { get; set; }

        [JsonProperty("postcode")]
        public string postcode { get; set; }

        [JsonProperty("phone")]
        public string phone { get; set; }

        [JsonProperty("number")]
        public string number { get; set; }

        [JsonProperty("neighborhood")]
        public string neighborhood { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("detail")]
        public string detail { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("complement")]
        public string complement { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }
    }

    public class Status
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("label")]
        public string label { get; set; }

        [JsonProperty("code")]
        public string code { get; set; }
    }
}

