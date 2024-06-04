namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order
{
    public class SearchOrders
    {
        public SearchOrders(int limit, int offset, DateTimeOffset from, DateTimeOffset to, string status)
        {
            Limit = limit;
            Offset = offset;
            From = from.ToString("yyyy-MM-ddTHH:mm:ss");
            To = to.ToString("yyyy-MM-ddTHH:mm:ss");
            Status = status;
        }

        public int Limit { get; set; }
        public int Offset { get; set; }
        public string From { get; private set; }
        public string To { get; private set; }

        public string Status { get; set; }
    }
}
