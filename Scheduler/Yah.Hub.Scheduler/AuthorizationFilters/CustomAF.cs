using System;
using Hangfire.Dashboard;

namespace Yah.Hub.Scheduler.AuthorizationFilters
{
    /// <summary>
    /// Used for Hangfire Dashboard only
    /// </summary>
    public class CustomAF : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return true;
        }
    }
}

