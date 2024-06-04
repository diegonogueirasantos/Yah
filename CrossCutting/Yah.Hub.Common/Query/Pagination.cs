namespace Yah.Hub.Common.Query
{
    public class Pagination
    {
        public Pagination(int offset, int limit)
        {
            this.Offset = offset;
            this.Limit = limit == default ? 10 : limit ;
        }

        public Pagination(int? limit)
        {
            if (!limit.HasValue)
                limit = 10;

            this.Limit = limit.Value;
        }

        public Pagination(int limit, string scrollId, TimeSpan scrollTime)
        {
            this.Limit = limit;
            this.ScrollId = scrollId;
            this.ScrollTime = scrollTime;
        }

        public int Offset { get; set; }
        public int Limit { get; set; }
        public string ScrollId { get; set; }
        public TimeSpan ScrollTime { get; private set; }
    }
}
