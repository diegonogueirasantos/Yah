namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class UpdateOrderStatus<T>
    {
        public UpdateOrderStatus(T data, string status, string orderId)
        {
            Data = data;
            Status = status;
            OrderId = orderId;
        }

        public T Data { get; set; }
        public string Status { get; set; }
        public string OrderId { get; set; }
    }
}
