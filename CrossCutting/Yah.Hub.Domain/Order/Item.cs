namespace Yah.Hub.Domain.Order
{
    public class Item
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SalesPrice { get; set; }
        public string Name { get; set; }
        public string ReferenceId { get; set; }
    }
}
