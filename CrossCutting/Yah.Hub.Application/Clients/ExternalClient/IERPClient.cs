using System;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order;

namespace Yah.Hub.Application.Clients.ExternalClient
{
	public interface IERPClient
	{
        public Task<HttpMarketplaceMessage> SendOrderAsync(MarketplaceServiceMessage<Order> message);
    }
}

