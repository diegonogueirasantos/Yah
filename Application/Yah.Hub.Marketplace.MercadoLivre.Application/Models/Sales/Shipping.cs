using Newtonsoft.Json;
using Yah.Hub.Domain.Order;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement;
using System.Net.Mail;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales
{
    public class Shipping
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("substatus")]
        public object Substatus { get; set; }

        [JsonProperty("date_created")]
        public DateTimeOffset? DateCreated { get; set; }

        [JsonProperty("last_updated")]
        public DateTimeOffset? LastUpdated { get; set; }

        [JsonProperty("declared_value")]
        public decimal? DeclaredValue { get; set; }

        [JsonProperty("dimensions")]
        public Dimensions Dimensions { get; set; }

        [JsonProperty("logistic")]
        public Logistic Logistic { get; set; }

        [JsonProperty("tracking_number")]
        public string TrackingNumber { get; set; }

        [JsonProperty("origin")]
        public Origin Origin { get; set; }

        [JsonProperty("destination")]
        public Destination Destination { get; set; }

        [JsonProperty("lead_time")]
        public LeadTime LeadTime { get; set; }

        [JsonProperty("shipping_option")]
        public ShippingOption ShippingOption { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("service_id")]
        public int? ServiceId { get; set; }
    }

    public class Destination
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("receiver_id")]
        public string ReceiverId { get; set; }

        [JsonProperty("receiver_name")]
        public string ReceiverName { get; set; }

        [JsonProperty("receiver_phone")]
        public string ReceiverPhone { get; set; }

        [JsonProperty("shipping_address")]
        public MeliAddress ShippingAddress { get; set; }
    }

    public class Dimensions
    {
        [JsonProperty("height")]
        public long? Height { get; set; }

        [JsonProperty("width")]
        public long? Width { get; set; }

        [JsonProperty("length")]
        public long? Length { get; set; }

        [JsonProperty("weight")]
        public long? Weight { get; set; }
    }

    public class ShippingOption
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cost")]
        public decimal? Cost { get; set; }

        [JsonProperty("estimated_delivery_time")]
        public EstimatedDeliveryTime EstimatedDeliveryTime { get; set; }

        [JsonProperty("shipping_method_id")]
        public long? ShippingMethodId { get; set; }
    }

    public class LeadTime
    {
        [JsonProperty("option_id")]
        public long? OptionId { get; set; }

        [JsonProperty("shipping_method")]
        public ShippingMethod ShippingMethod { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("cost")]
        public decimal? Cost { get; set; }

        [JsonProperty("cost_type")]
        public string CostType { get; set; }

        [JsonProperty("service_id")]
        public string ServiceId { get; set; }

        [JsonProperty("estimated_delivery_time")]
        public EstimatedDeliveryTime EstimatedDeliveryTime { get; set; }

        [JsonProperty("delivery_type")]
        public string DeliveryType { get; set; }
    }

    public class EstimatedDeliveryTime
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset? Date { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("offset")]
        public Offset Offset { get; set; }

        [JsonProperty("shipping")]
        public long? Shipping { get; set; }

        [JsonProperty("handling")]
        public long? Handling { get; set; }
    }

    public class Offset
    {
        [JsonProperty("date")]
        public DateTimeOffset? Date { get; set; }

        [JsonProperty("shipping")]
        public long? Shipping { get; set; }
    }

    public class ShippingMethod
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("deliver_to")]
        public string DeliverTo { get; set; }
    }

    public class Logistic
    {
        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Origin
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sender_id")]
        public long? SenderId { get; set; }

        [JsonProperty("shipping_address")]
        public MeliAddress ShippingAddress { get; set; }
    }
}
