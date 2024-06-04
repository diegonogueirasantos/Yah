using Yah.Hub.Common.Enums;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Domain.OrderStatus
{
    public class OrderStatus : IOrderReference
    {
        public string OrderId { get; set; }
        public string IntegrationOrderId { get; set; }
    }
}
