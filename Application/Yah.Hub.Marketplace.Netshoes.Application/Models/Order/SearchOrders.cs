namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class SearchOrders
    {
        public SearchOrders(int page, int size, DateTimeOffset from, DateTimeOffset to)
        {
            Page = page;
            Size = size;
            From = from.ToString("yyyy-MM-ddTHH:mm:ss");
            To = to.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public int Page { get; set; }
        public int Size { get; set; }
        public string From { get; private set; }
        public string To { get; private set; }
    }
}
