using System;
using Nest;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Data.Repositories.IntegrationMonitorRepository;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Order;

namespace Yah.Hub.Data.Repositories.OrderRepository
{
    public class OrderRepository : AbstractElasticSearchRepository<Order>, IOrderRepository
    {
        public OrderRepository(ILogger<Order> logger, IConfiguration configuration, IElasticClient client) : base(logger, configuration, client)
        {
        }

        public override string FormatKey(MarketplaceServiceMessage message)
        {
            return $"{message.Identity.GetVendorId()}-{message.Identity.GetTenantId()}-order";
        }
    }
}

