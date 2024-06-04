using System;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Scheduler.Services;

namespace Yah.Hub.Scheduler.Controllers
{
    [ApiController]
    public class SchedulerController : ControllerBase
    {
        private readonly ISchedulerService SchedulerService;
        private readonly ISecurityService SecurityService;

        public SchedulerController(ISchedulerService schedulerService, ISecurityService securityService)
        {
            this.SchedulerService = schedulerService;
            this.SecurityService = securityService;
        }

        [HttpGet]
        [Route("RequestExecution")]
        public IActionResult RequestExecution(string vendorId, string tenantId, string accountId, string marketplace, string uri)
        {
            var executed = this.SchedulerService.RequestExecution(vendorId, tenantId, accountId, marketplace, uri).GetAwaiter().GetResult();

            if (!executed)
            {
                return BadRequest();
            }

            return Ok($"Executed!!!");
        }

        [HttpPost]
        [Route("ReescheduleTasks")]
        public IActionResult ReescheduleTasks()
        {
            var executed =  this.SchedulerService.ReescheduleTasks(SecurityService.IssueWorkerIdentity().GetAwaiter().GetResult()).GetAwaiter().GetResult();

            if (!executed.Any())
            {
                return BadRequest();
            }

            return Ok(executed);
        }
    }
}

