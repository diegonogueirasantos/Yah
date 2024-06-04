using Yah.Hub.Common.Enums;
using Yah.Hub.Domain.Monitor.Query;

namespace Yah.Hub.Domain.Monitor
{
    public class MonitorStatusRequest : ElasticBasicScroll
    {
        public MonitorStatusRequest(int maxItemsPerExecution, TimeSpan scrollTime, int maxDaysMonitor) : base(maxItemsPerExecution, scrollTime)
        {
            MaxDaysMonitor = maxDaysMonitor;
        }

        public int MaxDaysMonitor { get; set; }
    }
}
