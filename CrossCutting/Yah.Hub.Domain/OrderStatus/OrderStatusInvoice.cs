using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Domain.OrderStatus
{
    public class OrderStatusInvoice : OrderStatus
    {
        public string Number { get; set; }
        public string Series { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
        public string IE { get; set; }
        public string CFOP { get; set; }
        public string Data { get; set; }
        public DateTimeOffset? Date { get; set; }
        public string ContentXML { get; set; }
        public string Cnpj { get; set; }
    }
}
