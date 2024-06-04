using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class MagaluOrder
    {
        [JsonProperty("IdOrder")]
        public string IdOrder { get; set; }

        [JsonProperty("IdOrderMarketplace")]
        public string IdOrderMarketplace { get; set; }

        [JsonProperty("InsertedDate")]
        public DateTimeOffset? InsertedDate { get; set; }

        [JsonProperty("PurchasedDate")]
        public DateTimeOffset? PurchasedDate { get; set; }

        [JsonProperty("ApprovedDate")]
        public DateTimeOffset? ApprovedDate { get; set; }

        [JsonProperty("UpdatedDate")]
        public DateTimeOffset? UpdatedDate { get; set; }

        [JsonProperty("MarketplaceName")]
        public string MarketplaceName { get; set; }

        [JsonProperty("StoreName")]
        public string StoreName { get; set; }

        [JsonProperty("UpdatedMarketplaceStatus")]
        public bool? UpdatedMarketplaceStatus { get; set; }

        [JsonProperty("InsertedErp")]
        public bool? InsertedErp { get; set; }

        [JsonProperty("EstimatedDeliveryDate")]
        public DateTimeOffset? EstimatedDeliveryDate { get; set; }

        [JsonProperty("CustomerPfCpf")]
        public string CustomerPfCpf { get; set; }

        [JsonProperty("ReceiverName")]
        public string ReceiverName { get; set; }

        [JsonProperty("CustomerPfName")]
        public string CustomerPfName { get; set; }

        [JsonProperty("CustomerPjCnpj")]
        public string CustomerPjCnpj { get; set; }

        [JsonProperty("CustomerPjCorporatename")]
        public string CustomerPjCorporatename { get; set; }

        [JsonProperty("DeliveryAddressStreet")]
        public string DeliveryAddressStreet { get; set; }

        [JsonProperty("DeliveryAddressAdditionalInfo")]
        public string DeliveryAddressAdditionalInfo { get; set; }

        [JsonProperty("DeliveryAddressZipcode")]
        public string DeliveryAddressZipcode { get; set; }

        [JsonProperty("DeliveryAddressNeighborhood")]
        public string DeliveryAddressNeighborhood { get; set; }

        [JsonProperty("DeliveryAddressCity")]
        public string DeliveryAddressCity { get; set; }

        [JsonProperty("DeliveryAddressReference")]
        public string DeliveryAddressReference { get; set; }

        [JsonProperty("DeliveryAddressState")]
        public string DeliveryAddressState { get; set; }

        [JsonProperty("DeliveryAddressNumber")]
        public string DeliveryAddressNumber { get; set; }

        [JsonProperty("TelephoneMainNumber")]
        public string TelephoneMainNumber { get; set; }

        [JsonProperty("TelephoneSecundaryNumber")]
        public string TelephoneSecundaryNumber { get; set; }

        [JsonProperty("TelephoneBusinessNumber")]
        public string TelephoneBusinessNumber { get; set; }

        [JsonProperty("TotalAmount")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("TotalTax")]
        public decimal? TotalTax { get; set; }

        [JsonProperty("TotalFreight")]
        public decimal? TotalFreight { get; set; }

        [JsonProperty("TotalDiscount")]
        public decimal? TotalDiscount { get; set; }

        [JsonProperty("CustomerMail")]
        public string CustomerMail { get; set; }

        [JsonProperty("CustomerBirthDate")]
        public DateTimeOffset? CustomerBirthDate { get; set; }

        [JsonProperty("CustomerPjIe")]
        public string CustomerPjIe { get; set; }

        [JsonProperty("OrderStatus")]
        public string OrderStatus { get; set; }

        [JsonProperty("InvoicedNumber")]
        public string InvoicedNumber { get; set; }

        [JsonProperty("InvoicedLine")]
        public string InvoicedLine { get; set; }

        [JsonProperty("InvoicedIssueDate")]
        public DateTimeOffset? InvoicedIssueDate { get; set; }

        [JsonProperty("InvoicedKey")]
        public string InvoicedKey { get; set; }

        [JsonProperty("InvoicedDanfeXml")]
        public string InvoicedDanfeXml { get; set; }

        [JsonProperty("ShippedTrackingUrl")]
        public string ShippedTrackingUrl { get; set; }

        [JsonProperty("ShippedTrackingProtocol")]
        public string ShippedTrackingProtocol { get; set; }

        [JsonProperty("ShippedEstimatedDelivery")]
        public DateTimeOffset? ShippedEstimatedDelivery { get; set; }

        [JsonProperty("ShippedCarrierDate")]
        public DateTimeOffset? ShippedCarrierDate { get; set; }

        [JsonProperty("ShippedCarrierName")]
        public string ShippedCarrierName { get; set; }

        [JsonProperty("ShipmentExceptionObservation")]
        public string ShipmentExceptionObservation { get; set; }

        [JsonProperty("ShipmentExceptionOccurrenceDate")]
        public DateTimeOffset? ShipmentExceptionOccurrenceDate { get; set; }

        [JsonProperty("DeliveredDate")]
        public DateTimeOffset? DeliveredDate { get; set; }

        [JsonProperty("ShippedCodeERP")]
        public string ShippedCodeErp { get; set; }

        [JsonProperty("Products")]
        public List<OrderProduct> Products { get; set; }

        [JsonProperty("Payments")]
        public List<OrderPayment> Payments { get; set; }
    }

    public class OrderPayment
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Installments")]
        public short? Installments { get; set; }

        [JsonProperty("Amount")]
        public decimal? Amount { get; set; }
    }

    public class OrderProduct
    {
        [JsonProperty("IdSku")]
        public string IdSku { get; set; }

        [JsonProperty("Quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("Price")]
        public decimal? Price { get; set; }

        [JsonProperty("Freight")]
        public decimal? Freight { get; set; }

        [JsonProperty("Discount")]
        public decimal? Discount { get; set; }

        [JsonProperty("IdOrderPackage")]
        public long? IdOrderPackage { get; set; }
    }

    public class PaginateOrder
    {
        [JsonProperty("Page")]
        public int Page { get; set; }

        [JsonProperty("PerPage")]
        public int PerPage { get; set; }

        [JsonProperty("Total")]
        public int Total { get; set; }

        [JsonProperty("Orders")]
        public MagaluOrder[] Orders { get; set; }
    }
    public class ShipmentOrder
    {
        [JsonProperty("Order")]
        public string Order { get; set; }
        [JsonProperty("TrackingCode")]
        public string TrackingCode { get; set; }
        [JsonProperty("TrackingUrl")]
        public string TrackingUrl { get; set; }
    }
}

