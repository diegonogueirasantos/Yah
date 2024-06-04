using DateRange = Yah.Hub.Common.OrderRequest.DateRange;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales
{
    public class OrderQueryRequest : DateRange
    {
        public OrderQueryRequest(string sellerId, int offset, int limit, DateTime from, DateTime to) : base(from, to)
        {
            SellerId = sellerId;
            Offset = offset;
            Limit = limit;
        }

        public string SellerId { get; set; }
        public string Sort { get; set; } = "date_asc";
        public int Offset { get; set; }
        public int Limit { get; set; }
        public Dictionary<string, string> Status { get; set; } = new Dictionary<string, string>();

    }



}
