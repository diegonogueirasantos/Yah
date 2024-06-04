namespace Yah.Hub.Domain.Order
{
    public class OrderWrapper
    {
        public OrderWrapper(Order order, object marketplaceOrder)
        {
            Order = order;
            MarketplaceOrder = marketplaceOrder;
        }

        public Order Order { get; set; }

        public object MarketplaceOrder { get; set; }
    }
}
