using System;
using Amazon.DynamoDBv2.Model;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Identity;
using Identity = Yah.Hub.Common.Identity.Identity;

namespace Yah.Hub.Scheduler.Services
{
	public interface ISchedulerService
	{
        public  Task<bool> RequestExecution(string vendorId, string tenantId, string accountId, string marketplace, string uri);
        public  Task<List<string>> ReescheduleTasks(Identity identity);
    }
}


