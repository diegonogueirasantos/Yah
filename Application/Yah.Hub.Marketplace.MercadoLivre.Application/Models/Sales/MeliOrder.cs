using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales
{
    public class MeliOrder
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("date_created")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonIgnore]
        public DateTimeOffset LastUpdateDate => new[] { DateLastUpdated, LastUpdated }.Max() ?? DateTimeOffset.MinValue;

        [JsonProperty("date_last_updated")]
        public DateTimeOffset? DateLastUpdated { get; set; }

        [JsonProperty("last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("order_items")]
        public List<OrderItem> OrderItems { get; set; }

        [JsonProperty("payments")]
        public List<Payment> Payments { get; set; }

        [JsonProperty("shipping")]
        public Shipping Shipping { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("buyer")]
        public User Buyer { get; set; }

        [JsonProperty("seller")]
        public User Seller { get; set; }

        [JsonProperty("pickup_id")]
        public long? PickupId { get; set; }

        [JsonProperty("pack_id")]
        public long? PackId { get; set; }

        public MeliFiscalDocumentsResult FiscalDocuments { get; set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public Phone Phone { get; set; }

        [JsonProperty("alternative_phone")]
        public Phone AlternativePhone { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("billing_info")]
        public BillingInfo BillingInfo { get; set; }
    }

    public class Phone
    {
        [JsonProperty("area_code")]
        public string AreaCode { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("verified")]
        public bool? Verified { get; set; }
    }

    public class BillingInfoRequest
    {
        [JsonProperty("billing_info")]
        public BillingInfo BillingInfo { get; set; }
    }

    public class BillingInfo
    {
        [JsonProperty("doc_type")]
        public string DocType { get; set; }

        [JsonProperty("doc_number")]
        public string DocNumber { get; set; }

        [JsonProperty("additional_info")]
        public List<AdditionalInfo> AdditionalInfo { get; set; }
    }

    public class AdditionalInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class OrderItem
    {
        [JsonProperty("item")]
        public Item Item { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("unit_price")]
        public decimal? UnitPrice { get; set; }

        [JsonProperty("full_unit_price")]
        public decimal? FullUnitPrice { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        /// <summary>
        /// Tipo do anúncio (Premium ou Clássico)
        /// </summary>
        [JsonIgnore]
        public string ListingType { get; set; }

        /// <summary>
        /// Indica se o anúncio possui retirada no local
        /// </summary>
        [JsonIgnore]
        public bool HasLocalPickup { get; set; }
    }

    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("variation_id")]
        public string VariationId { get; set; }

        [JsonProperty("seller_custom_field")]
        public string SellerCustomField { get; set; }

        [JsonProperty("warranty")]
        public string Warranty { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("seller_sku")]
        public string SellerSku { get; set; }
    }

    public class Payment
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("payment_method_id")]
        public string PaymentMethodId { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("installments")]
        public short Installments { get; set; }

        [JsonProperty("operation_type")]
        public string OperationType { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("available_actions")]
        public List<string> AvailableActions { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }

        [JsonProperty("transaction_amount")]
        public decimal? TransactionAmount { get; set; }

        [JsonProperty("taxes_amount")]
        public decimal? TaxesAmount { get; set; }

        [JsonProperty("shipping_cost")]
        public decimal? ShippingCost { get; set; }

        [JsonProperty("coupon_amount")]
        public decimal? CouponAmount { get; set; }

        [JsonProperty("overpaid_amount")]
        public decimal? OverpaidAmount { get; set; }

        [JsonProperty("total_paid_amount")]
        public decimal? TotalPaidAmount { get; set; }

        [JsonProperty("installment_amount")]
        public decimal? InstallmentAmount { get; set; }

        [JsonProperty("date_approved")]
        public DateTimeOffset? DateApproved { get; set; }

        [JsonProperty("authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty("date_created")]
        public DateTimeOffset? DateCreated { get; set; }

        [JsonProperty("date_last_modified")]
        public DateTimeOffset? DateLastModified { get; set; }
    }

    public class OrderShipping
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("service_id ")]
        public string ServiceId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("shipping_option")]
        public ShippingOption ShippingOption { get; set; }
    }

    public class MeliFiscalDocumentsResult
    {
        [JsonProperty("pack_id")]
        public long PackId { get; set; }
        [JsonProperty("fiscal_documents")]
        public MeliFiscalDocument[] FiscalDocuments { get; set; }
    }
    public class MeliFiscalDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("filename")]
        public string FileName { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("file_type")]
        public string FileType { get; set; }
    }
}
