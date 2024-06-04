using System;
using System.Net.Http;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Extensions;
using Nest;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.Identity;
using Microsoft.Extensions.Logging;

namespace Yah.Hub.Scheduler.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SchedulerService> _logger;
        private readonly IAccountConfigurationService AccountConfigurationService;
        private readonly HttpClient HttpClient;
        private readonly IHangfireService HangfireService;
        private readonly ISecurityService SecurityService;

        public SchedulerService(
            HttpClient httpClient, 
            IAccountConfigurationService accountConfigurationService, 
            IHangfireService hangfireService, 
            IConfiguration configuration, 
            ILogger<SchedulerService> logger,
            ISecurityService securityService)
        {
            this.HttpClient = httpClient;
            this._configuration = configuration;
            this._logger = logger;
            this.AccountConfigurationService = accountConfigurationService;
            this.HangfireService = hangfireService;
            this.SecurityService = securityService;
        }

        public async Task<bool> RequestExecution(string vendorId, string tenantId, string accountId, string markteplace, string uri)
        {

            var requestMessage = new HttpRequestMessage();
            var result = default(bool);

            try
            {
                requestMessage.SetHeaders(new Dictionary<string, string>() {
                    { "x-VendorId", vendorId },
                    { "x-TenantId", tenantId},
                    { "x-AccountId", accountId},
                    { "x-Marketplace", markteplace}
                });
                
                requestMessage.RequestUri = new Uri($"{uri}");

                var requestResult = await this.HttpClient.SendAsync(requestMessage);
                result = requestResult.IsSuccessStatusCode;

                this._logger.LogInformation($"REQUEST RESULT: {vendorId}-{tenantId}-{accountId} - {markteplace} - {uri} - {requestResult.StatusCode} - {requestResult.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return false;
            }

            return result;
        }

        public async Task<List<string>> ReescheduleTasks(Identity identity)
        {
            var result = new List<string>();

            try
            {
                var accountResult = await this.AccountConfigurationService.GetSummary();

                foreach(var item in accountResult.OrderBy(x => x.VendorId).ThenBy(x => x.TenantId).ThenBy(x => x.AccountId))
                {
                    // marketplace
                    var marketplace = item.Marketplace;

                    var credential = await this.SecurityService.ImpersonateClaimIdentity(identity, item.VendorId, item.TenantId, item.AccountId);

                    // get services of marketplace and enqueu tasks
                    foreach (var service in this._configuration.GetSection($"Marketplaces:{marketplace.ToString()}:Tasks").Get<Dictionary<string, string>>())
                    {
                        // TODO: PUT MARKETPLACE ON PATH (ROUTE 53)
                        var uri = $"{this._configuration[$"Marketplaces:{marketplace.ToString()}:Uri"]}/{service.Key}";

                        // creates base message
                        var message = new Common.Marketplace.MarketplaceServiceMessage<(System.Linq.Expressions.Expression<Action> executedMethod, string taskName, string periodicity)>(credential, item, (() => this.RequestExecution(item.VendorId, item.TenantId, item.AccountId, item.Marketplace.ToString() , uri), service.Key, service.Value));

                        // adds or update task
                        var taskResult = await this.HangfireService.AddOrUpdateTask(message);
                        if (taskResult)
                            result.Add(this.HangfireService.GetTaskKeys(message));
                    }

                    // get general tasks
                    foreach (var service in this._configuration.GetSection($"Marketplaces:General:Tasks").Get<Dictionary<string, string>>())
                    {
                        // TODO: PUT MARKETPLACE ON PATH (ROUTE 53)
                        var uri = $"{this._configuration[$"Marketplaces:General:Uri"]}/{service.Key}";

                        // creates base message
                        var message = new Common.Marketplace.MarketplaceServiceMessage<(System.Linq.Expressions.Expression<Action> executedMethod, string taskName, string periodicity)>(credential, item, (() => this.RequestExecution(item.VendorId, item.TenantId, item.AccountId, item.Marketplace.ToString(), uri), service.Key, service.Value));
                        
                        // adds or update task
                        var taskResult = await this.HangfireService.AddOrUpdateTask(message);
                        if (taskResult)
                            result.Add(this.HangfireService.GetTaskKeys(message));
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return result;
            }

            return result;
        }
    }
}

