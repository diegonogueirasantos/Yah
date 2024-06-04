using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Domain.OrderStatus
{
    public class OrderStatusShipment : OrderStatus
    {
        public string TrackingCode { get; set; }
        public string CarrierName { get; set; }
        public string DeliveryMethod { get; set; }
        public string TrackingUrl { get; set; }
        public DateTimeOffset? EstimatedTimeArrival { get; set; }
        public DateTimeOffset? Date { get; set; }
        public string Alias { get; set; }
        public string Cte { get; set; }
        public string Cnpj { get; set; }
    }
}
