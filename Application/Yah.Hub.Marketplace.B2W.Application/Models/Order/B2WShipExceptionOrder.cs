using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models.Order
{
    public class B2WShipExceptionOrder
    {
        [JsonProperty("shipment_exception")]
        public ShipmentException ShipmentException { get; set; }

        [JsonIgnore]
        public string OrderId { get; set; }
    }

    public class ShipmentException
    {
        [JsonProperty("occurrence_date")]
        public string OccurrenceDate { get; set; }

        [JsonProperty("observation")]
        public string Observation { get; set; }
    }

}
