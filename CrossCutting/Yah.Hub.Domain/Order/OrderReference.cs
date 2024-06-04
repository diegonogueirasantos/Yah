using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Domain.Order
{
    public class OrderReference : IOrderReference
    {
        public OrderReference( string orderReference) 
        {
            this.IntegrationOrderId = orderReference;
        }

        public string IntegrationOrderId { get; set; }
    }
}
