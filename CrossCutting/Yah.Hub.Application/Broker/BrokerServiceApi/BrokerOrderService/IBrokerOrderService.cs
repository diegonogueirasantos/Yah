using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.ShipmentLabel;

namespace Yah.Hub.Application.Broker.BrokerServiceApi.BrokerOrderService
{
    public interface IBrokerOrderService
    {
        public Task<ServiceMessage> UpdateOrder<T>(MarketplaceServiceMessage<T> message);
        public Task<ServiceMessage<ShipmentLabel>> GetShipmentLabel(MarketplaceServiceMessage<IOrderReference> message);
    }
}
