namespace Yah.Hub.Common.Query
{
    public abstract class BaseQuery
    {
        public Pagination Paging { get; set; } = new Pagination(0,10);
    }
}
