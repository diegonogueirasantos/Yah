using System;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Domain.Monitor;
using Yah.Hub.Marketplace.Application.Broker.Messages;

namespace Yah.Hub.Marketplace.Application.Catalog
{
    public interface ICatalogService
    {
        // sync methods 
        public Task<ServiceMessage> ExecuteProductCommand(ServiceMessage<CommandMessage<Product>> serviceMessage);
        public Task<ServiceMessage> ExecuteProductPriceCommand(ServiceMessage<CommandMessage<ProductPrice>> serviceMessage);
        public Task<ServiceMessage> ExecuteProductInventoryCommand(ServiceMessage<CommandMessage<ProductInventory>> serviceMessage);
        public Task<ServiceMessage> ExecuteProductIntegrationStatusCommand(ServiceMessage<CommandMessage<RequestProductState>> serviceMessage);

        // consumers
        public Task<ServiceMessage> ConsumeProductCommand(ServiceMessage serviceMessage);
        public Task<ServiceMessage> ConsumeProductPriceCommand(ServiceMessage serviceMessage);
        public Task<ServiceMessage> ConsumeProductInventoryCommand(ServiceMessage serviceMessage);
        public Task<ServiceMessage> ConsumeProductRequestState(ServiceMessage serviceMessage);

        // producers
        public Task<ServiceMessage> EnqueueProductCommand(ServiceMessage<CommandMessage<Product>> serviceMessage);
        public Task<ServiceMessage> EnqueueProductPriceCommand(ServiceMessage<CommandMessage<ProductPrice>> serviceMessage);
        public Task<ServiceMessage> EnqueueProductInventoryCommand(ServiceMessage<CommandMessage<ProductInventory>> serviceMessage);
    }
}

