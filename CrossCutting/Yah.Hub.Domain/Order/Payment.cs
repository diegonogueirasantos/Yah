namespace Yah.Hub.Domain.Order
{
    public class Payment
    {
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int Installments { get; set; }
        public decimal InterestAmount { get; set; }
    }
}
