using System;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Data.Repositories.OrderRepository;
using Yah.Hub.Domain.Order;

namespace Yah.Hub.Application.Services.OrderService
{
	public class OrderService : AbstractService, IOrderService
    {
		private readonly IOrderRepository OrderRepository;

		public OrderService(IConfiguration configuration, ILogger<OrderService> logger, IOrderRepository orderRepository) : base(configuration, logger) 
		{
			this.OrderRepository = orderRepository;
		}

		public async Task<ServiceMessage<Order>> SaveOrder(MarketplaceServiceMessage<Order> serviceMessage)
		{
			return await this.OrderRepository.SaveAsync(serviceMessage);
		}

        public async Task<ServiceMessage<Order>> GetOrderById(MarketplaceServiceMessage<string> serviceMessage)
        {
            return await this.OrderRepository.GetAsync(serviceMessage);
        }
    }
}