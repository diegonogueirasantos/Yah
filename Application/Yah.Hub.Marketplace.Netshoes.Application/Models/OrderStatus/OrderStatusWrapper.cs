using Newtonsoft.Json;
using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus
{
    public class OrderStatusWrapper<T>
    {
        public string Code { get; set; }

        public string IntegrationOrderId { get; set; }

        public T Data { get; set; }

        public string Status { get; set; }
    }
}
