using Yah.Hub.Common.Enums;

namespace Yah.Hub.Domain.Order
{
    public class Logistic
    {
        public decimal TotalAmount { get; set; }
        public short ETA { get; set; }
        public DeliveryLogisticType DeliveryLogisticType { get; set; }
        public int CarrierId { get; set; }
        public string CarrierName { get; set; }
        public string CarrierReference { get; set; }
        public string TrackingCode { get; set; }
        public string TrackingURL { get; set; }
    }
}
