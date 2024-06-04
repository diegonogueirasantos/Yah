using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluShipmentException : MagaluOrderStatus
    {
        [JsonProperty("ShipmentExceptionObservation")]
        public string ShipmentExceptionObservation { get; set; }

        [JsonProperty("ShipmentExceptionOccurrenceDate")]
        public DateTimeOffset ShipmentExceptionOccurrenceDate { get; set; }
    }
}
