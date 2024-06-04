using System;
using Newtonsoft.Json;
using Yah.Hub.Domain.OrderStatus;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluShipOrder : MagaluOrderStatus
    {
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
    }
}

