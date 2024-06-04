using System;
using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Domain.Order;

namespace Yah.Hub.Data.Repositories.OrderRepository
{
	public interface IOrderRepository : IElasticSearchBaseRepository<Order>
    {
		
	}
}

