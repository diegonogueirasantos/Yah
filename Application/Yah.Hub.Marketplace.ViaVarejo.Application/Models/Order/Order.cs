using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("orderSiteId")]
        public string OrderSiteId { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("paymentType")]
        public string PaymentType { get; set; }

        [JsonProperty("purchasedAt")]
        public string PurchasedAt { get; set; }

        [JsonProperty("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonProperty("status")]
        public OrderStatusAlias Status { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("totalDiscountAmount")]
        public decimal TotalDiscountAmount { get; set; }

        [JsonProperty("seller")]
        public Seller Seller { get; set; }

        [JsonProperty("billing")]
        public Address? BillingAddress { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("freight")]
        public Freight Freight { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("itemsSummary")]
        public itemSummary[] ItemsSummary { get; set; }

        [JsonProperty("shipping")]
        public Address ShippingAddress { get; set; }

        [JsonProperty("trackings")]
        public Tracking[] Trackings { get; set; }

        [JsonProperty("promisedExpeditionDate")]
        public string PromisedExpeditionDate { get; set; }

        [JsonProperty("promisedDeliveryDate")]
        public string PromisedDeliveryDate { get; set; }

        [JsonProperty("orderDelayed")]
        public bool OrderDelayed { get; set; }

        [JsonProperty("cnpjViaVarejo")]
        public string CnpjViaVarejo { get; set; }

        [JsonProperty("payments")]
        public Payment[] Payment { get; set; }
    }

    #region [InnerClass]

    #region [Payment]
    public class Payment
    {
        /// <summary>
        /// Forma de pagamento. 1= Cartão de crédito, 2= Boleto bancário/ Carnê digital (CDC), 4= Cupom de desconto/ Vale e 5= Débito online/ Pix.
        /// </summary>
        [JsonProperty("paymentType")]
        public int PaymentType { get; set; }

        [JsonProperty("numberInstallments")]
        public int Installments { get; set; }

        [JsonProperty("sefaz")]
        public Sefaz Sefaz { get; set; }
    }

    public class Sefaz
    {
        [JsonProperty("paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }

        [JsonProperty("creditCardNetwork")]
        public CreditCardNetwork CreditCardNetwork { get; set; }

        [JsonProperty("transactionType")]
        public TransactionType TransactionType { get; set; }

        [JsonProperty("acquirer")]
        public Acquirer Acquirer { get; set; }

        [JsonProperty("purchaseIntermediary")]
        public PurchaseIntermediary PurchaseIntermediary { get; set; }
    }

    public class PurchaseIntermediary
    {
        [JsonProperty("cnpj")]
        public string cnpj { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Acquirer
    {
        [JsonProperty("cnpj")]
        public string cnpj { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TransactionType
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class CreditCardNetwork
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PaymentMethod
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    #endregion

    #region [Seller]
    public class Seller
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    #endregion

    #region [Address]
    public class Address
    {
        [JsonProperty("address")]
        public string Street { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("complement")]
        public string Complement { get; set; }

        [JsonProperty("quarter")]
        public string Quarter { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("countryId")]
        public string CountryId { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }
    }
    #endregion

    #region [Customer]
    public class Customer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("documentNumber")]
        public string DocumentNumber { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("birthDate")]
        public string BirthDate { get; set; }

        [JsonProperty("phones")]
        public Phone[] Phones { get; set; }
    }
    #endregion

    #region [Phone]
    public class Phone
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
    #endregion

    #region [Freight]
    public class Freight
    {
        [JsonProperty("actualAmount")]
        public string ActualAmount { get; set; }

        [JsonProperty("chargedAmount")]
        public string ChargedAmount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
    #endregion

    #region [Item]
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("skuSellerId")]
        public string SkuSellerId { get; set; }

        [JsonProperty("deliveryId")]
        public long DeliveryId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("salePrice")]
        public decimal SalePrice { get; set; }

        [JsonProperty("sent")]
        public bool IsSent { get; set; }

        [JsonProperty("freight")]
        public FreightItem FreightItem { get; set; }

        [JsonProperty("serviceType")]
        public string ServiceType { get; set; }
    }

    public class FreightItem
    {
        [JsonProperty("actualAmount")]
        public decimal ActualAmount { get; set; }

        [JsonProperty("chargedAmount")]
        public decimal ChargedAmount { get; set; }

        [JsonProperty("transitTime")]
        public int TransitTime { get; set; }

        [JsonProperty("crossDockingTime")]
        public int CrossDockingTime { get; set; }

        [JsonProperty("additionalInfo")]
        public string AdditionalInfo { get; set; }

        [JsonProperty("envvias")]
        public bool IsEnvvias { get; set; }

        [JsonProperty("warehouse")]
        public Warehouse Warehouse { get; set; }

        [JsonProperty("fulfillment")]
        public bool IsFulfillment { get; set; }
    }

    public class itemSummary
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("skuSellerId")]
        public string SkuSellerId { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("freight")]
        public FreightItem FreightItemsSummary { get; set; }
    }

    public class Warehouse
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }
    #endregion

    #region [Tracking]
    public class Tracking
    {
        [JsonProperty("items")]
        public TrackingItems[] trackingItems { get; set; }

        [JsonProperty("controlPoint")]
        public string ControlPoint { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("occuredAt")]
        public string OccuredAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sellerDeliveryId")]
        public string SellerDeliveryId { get; set; }

        [JsonProperty("carrier")]
        public Carrier Carrier { get; set; }

        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }
    }

    public class TrackingItems
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public class Carrier
    {
        [JsonProperty("cnpj")]
        public string Cnpj { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Invoice
    {
        [JsonProperty("cnpj")]
        public string Cnpj { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("serie")]
        public string Serie { get; set; }

        [JsonProperty("issuedAt")]
        public string IssuedAt { get; set; }

        [JsonProperty("accessKey")]
        public string AccessKey { get; set; }

        [JsonProperty("linkXml")]
        public string LinkXml { get; set; }

        [JsonProperty("linkDanfe", NullValueHandling = NullValueHandling.Ignore)]
        public string LinkDanfe { get; set; }
    }
    #endregion

    #region [Status]
    public enum OrderStatusAlias
    {
        /// <summary>
        /// Pagamento Pendente
        /// </summary>
        PEN,
        /// <summary>
        /// Pagamento aprovado
        /// </summary>
        PAY,
        /// <summary>
        /// Nota fiscal cadastrada
        /// </summary>
        RIN,
        /// <summary>
        /// Nota fiscal parcialmente cadastrada
        /// </summary>
        PRI,
        /// <summary>
        /// Enviado
        /// </summary>
        SHP,
        /// <summary>
        /// Parcialmente enviado
        /// </summary>
        PSH,
        /// <summary>
        /// Entregue
        /// </summary>
        DLV,
        /// <summary>
        /// Parcialmente entregue
        /// </summary>
        PDL,
        /// <summary>
        /// Cancelado
        /// </summary>
        CAN,
        /// <summary>
        /// Devolvido
        /// </summary>
        DVC

    }
    #endregion

    #endregion
}
