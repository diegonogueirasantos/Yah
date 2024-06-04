namespace Yah.Hub.Domain.Monitor.Query
{
    public class ElasticBasicScroll
    {
        public ElasticBasicScroll(int maxItemsPerExecution, TimeSpan scrollTime)
        {
            MaxItemsPerExecution = maxItemsPerExecution;
            ScrollTime = scrollTime;
        }

        public int MaxItemsPerExecution { get; set; }
        public TimeSpan ScrollTime { get; set; }
    }
}
