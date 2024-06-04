using System;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order;

namespace Yah.Hub.Application.Services.OrderService
{
	public interface IOrderService
	{
        public Task<ServiceMessage<Order>> SaveOrder(MarketplaceServiceMessage<Order> serviceMessage);
        public Task<ServiceMessage<Order>> GetOrderById(MarketplaceServiceMessage<string> serviceMessage);
    }
}

