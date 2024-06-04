namespace Yah.Hub.Common.Query
{
    public class QueryResultBase<T>
    {
        public T[] Docs { get; set; }

        public int Count { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }
    }
}
