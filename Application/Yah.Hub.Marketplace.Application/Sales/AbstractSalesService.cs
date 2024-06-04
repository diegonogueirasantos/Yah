using System;
using System.Linq;
using Nest;
using Yah.Hub.Application.Clients.ExternalClient;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Application.Services.OrderService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Marketplace.Interfaces;
using Yah.Hub.Common.OrderRequest;
using Yah.Hub.Common.Security;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.ServiceMessage.Interfaces;
using Yah.Hub.Common.Services;
using Yah.Hub.Common.Services.CacheService;
using Yah.Hub.Common.Services.CachingService;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.Order.Interface;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Domain.ShipmentLabel;
using Yah.Hub.Marketplace.Application.Broker.Interface;
using Yah.Hub.Marketplace.Application.Broker.Messages;
using StackExchange.Redis;
using DateRange = Yah.Hub.Common.OrderRequest.DateRange;
using Order = Yah.Hub.Domain.Order.Order;

namespace Yah.Hub.Marketplace.Application.Sales
{
    public abstract class AbstractSalesService : AbstractMarketplaceService, ISalesService
    {
        protected IAccountConfigurationService ConfigurationService { get; }
        protected ICacheService CacheService { get; set; }
        protected IOrderService OrderService { get; set; }
        protected IBrokerService BrokerService { get; set; }
        protected IERPClient ERPClient { get; set; }

        public AbstractSalesService(IConfiguration configuration, ILogger<AbstractSalesService> logger, IAccountConfigurationService configurationService, ICacheService cacheService, IOrderService orderService, IBrokerService brokerService, IERPClient eRPClient) : base(configuration, logger)
        {
            this.ConfigurationService = configurationService;
            this.CacheService = cacheService;
            this.OrderService = orderService;
            this.BrokerService = brokerService;
            this.ERPClient = eRPClient;
        }

        public abstract Task<ServiceMessage<List<Order>>>  GetOrdersToIntegrateFromMarketplaceAsync(MarketplaceServiceMessage<Common.OrderRequest.DateRange> message);

        public abstract Task<ServiceMessage<OrderWrapper>> TryGetOrderFromMarketplaceAsync(MarketplaceServiceMessage<IOrderReference> message);

        public abstract Task<ServiceMessage> TryCancelOrderAsync(MarketplaceServiceMessage<(OrderStatusCancel status, Order order)> message);

        public abstract Task<ServiceMessage> TryInvoiceOrderAsync(MarketplaceServiceMessage<(OrderStatusInvoice status, Order order)> message);

        public abstract Task<ServiceMessage> TryShipOrderAsync(MarketplaceServiceMessage<(OrderStatusShipment status, Order order)> message);

        public abstract Task<ServiceMessage> TryDeliveryOrderAsync(MarketplaceServiceMessage<(OrderStatusDelivered status, Order order)> message);

        public abstract Task<ServiceMessage> TryShipExceptionOrderAsync(MarketplaceServiceMessage<(OrderStatusShipException status, Order order)> message);

        public abstract Task<ServiceMessage<ShipmentLabel>> GetShipmentLabelAsync(MarketplaceServiceMessage<string> message);

        public abstract Task<ServiceMessage> DequeueOrderFromMarketplaceAsync(MarketplaceServiceMessage<Order> message);


        public virtual async Task<ServiceMessage> NotifyOrderIntegrated(MarketplaceServiceMessage<Order> message)
        {
            return ServiceMessage.CreateValidResult(message.Identity);
        }

        public virtual async Task<ServiceMessage<Order>> GetOrderAsync(ServiceMessage<string> message)
        {
            #region [Code]

            var result = new ServiceMessage<Order>(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                var orderResult = await this.TryGetOrderFromMarketplaceAsync(new MarketplaceServiceMessage<IOrderReference>(configResult.Identity, configResult.Data, new OrderReference(message.Data)));

                if (!orderResult.IsValid)
                {
                    result.WithErrors(orderResult.Errors);
                    return result;
                }

                if (orderResult.Data == null)
                {
                    result.WithError(new Error("Não foi possível recuperar o pedido", "Pedido Inexistente", ErrorType.Business));
                    return result;
                }

                result.Data = orderResult.Data.Order;

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to get order: {message.Data}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;

            #endregion
        }


        public virtual async Task<ServiceMessage> ChangeOrderStatusInvoiceAsync(ServiceMessage<OrderStatusInvoice> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                //var orderResult = await this.OrderService.GetOrderById(new MarketplaceServiceMessage<string>(configResult.Identity, configResult.Data, message.Data.OrderId.ToString()));

                //if (!orderResult.IsValid)
                //{
                //    result.WithErrors(orderResult.Errors);
                //   return result;
                //}

                //if (orderResult.Data == null)
                //{
                //    result.WithError(new Error("Não foi possível recuperar o pedido", "Pedido Inexistente", ErrorType.Business));
                //    return result;
                //}
                var order = await this.GetOrderAsync(new ServiceMessage<string>(message.Identity, message.Data.OrderId));

                var content = new MarketplaceServiceMessage<(OrderStatusInvoice status, Order order)>(configResult.Identity, configResult.Data, (message.Data, order.Data));

                return await this.TryInvoiceOrderAsync(content);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to invoice order: {message.Data.OrderId}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> ChangeOrderStatusShipAsync(ServiceMessage<OrderStatusShipment> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                //var orderResult = await this.OrderService.GetOrderById(new MarketplaceServiceMessage<string>(configResult.Identity, configResult.Data, message.Data.OrderId.ToString()));

                //if (!orderResult.IsValid)
                //{
                //    result.WithErrors(orderResult.Errors);
                //    return result;
                //}

                //if (orderResult.Data == null)
                //{
                //    result.WithError(new Error("Não foi possível recuperar o pedido", "Pedido Inexistente", ErrorType.Business));
                //    return result;
                //}

                var order = await this.GetOrderAsync(new ServiceMessage<string>(message.Identity, message.Data.OrderId));

                var content = new MarketplaceServiceMessage<(OrderStatusShipment status, Order order)>(configResult.Identity, configResult.Data, (message.Data, order.Data));

                return await this.TryShipOrderAsync(content);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to ship order: {message.Data.OrderId}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> ChangeOrderStatusShipExceptionAsync(ServiceMessage<OrderStatusShipException> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                var orderResult = await this.OrderService.GetOrderById(new MarketplaceServiceMessage<string>(configResult.Identity, configResult.Data, message.Data.OrderId.ToString()));

                if (!orderResult.IsValid)
                {
                    result.WithErrors(orderResult.Errors);
                    return result;
                }

                if (orderResult.Data == null)
                {
                    result.WithError(new Error("Não foi possível recuperar o pedido", "Pedido Inexistente", ErrorType.Business));
                    return result;
                }

                var content = new MarketplaceServiceMessage<(OrderStatusShipException status, Order order)>(configResult.Identity, configResult.Data, (message.Data, orderResult.Data));

                return await this.TryShipExceptionOrderAsync(content);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to ship order: {message.Data.OrderId}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> ChangeOrderStatusCancelAsync(ServiceMessage<OrderStatusCancel> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                //var orderResult = await this.OrderService.GetOrderById(new MarketplaceServiceMessage<string>(configResult.Identity, configResult.Data, message.Data.OrderId.ToString()));

                //if (!orderResult.IsValid)
                //{
                //    result.WithErrors(orderResult.Errors);
                //    return result;
                //}

                //if (orderResult.Data == null)
                //{
                //    result.WithError(new Error("Não foi possível recuperar o pedido", "Pedido Inexistente", ErrorType.Business));
                //    return result;
                //}

                var order = await this.GetOrderAsync(new ServiceMessage<string>(message.Identity, message.Data.OrderId));


                var content = new MarketplaceServiceMessage<(OrderStatusCancel status, Order order)>(configResult.Identity, configResult.Data, (message.Data, order.Data));

                return await this.TryCancelOrderAsync(content);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to cancel order: {message.Data.OrderId}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> ChangeOrderStatusDeliveryAsync(ServiceMessage<OrderStatusDelivered> message)
        {
            #region [Code]

            var result = new ServiceMessage(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                //var orderResult = await this.OrderService.GetOrderById(new MarketplaceServiceMessage<string>(configResult.Identity, configResult.Data, message.Data.OrderId.ToString()));

                //if (!orderResult.IsValid)
                //{
                //    result.WithErrors(orderResult.Errors);
                //   return result;
                //}

                //if (orderResult.Data == null)
                //{
                //    result.WithError(new Error("Não foi possível recuperar o pedido", "Pedido Inexistente", ErrorType.Business));
                //    return result;
                //}

                var order = await this.GetOrderAsync(new ServiceMessage<string>(message.Identity, message.Data.OrderId));

                var content = new MarketplaceServiceMessage<(OrderStatusDelivered status, Order order)>(configResult.Identity, configResult.Data, (message.Data, order.Data));

                return await this.TryDeliveryOrderAsync(content);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to delivery order: {message.Data.OrderId}");
                Logger.LogCustomCritical(error, message.Identity, message.Data);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> RetrieveOrdersFromMarketplaceAsync(ServiceMessage message, bool isScan)
        {
            #region Code

            var result = new ServiceMessage<List<Order>>(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }
            
            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message.AsMarketplaceServiceMessage(message.Identity, GetMarketplace()));

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                // build order message
                var orderRequestMessage = new MarketplaceServiceMessage<OrderRequest>(configResult.Identity, configResult.Data);

                // build order request message
                orderRequestMessage.WithData(new OrderRequest(
                    isScan,
                    new DateRange()
                    {
                        // TODO: PASS TO CONFIG
                        From = isScan ? DateTime.Now.AddDays(-30) : DateTime.Now.AddHours(-8),
                        To = DateTime.Now
                    },
                    ScrollType.OffsetLimit));

                using (var lockExecution = new LockState(this.CacheService, new ServiceMessage<(string key, DateTimeOffset timestamp, TimeSpan lockTimeout)>(orderRequestMessage.Identity, (GetCacheKey(new ServiceMessage<(bool isScan, bool isState)>(message.Identity, (isScan, false))), DateTimeOffset.Now, TimeSpan.FromMinutes(10)))))
                {
                    if (!lockExecution.IsLocked)
                    {
                        result.WithError(new Error("Locked!", "Method or Operation is already in execution", ErrorType.Technical));
                        return result;
                    }

                    // retrieve state
                    var currentState = await this.LoadState(message, orderRequestMessage.Data.IsScan);
                    if (currentState.IsValid && currentState.Data != null)
                        orderRequestMessage.Data = currentState.Data;

                    // TODO: Create a merge method
                    var marketplaceOrderRequestMessage = new MarketplaceServiceMessage<DateRange>(orderRequestMessage.Identity,configResult.Data, orderRequestMessage.Data.Range);

                    // retrieve orders
                    var orderResult = await GetOrdersToIntegrateFromMarketplaceAsync(marketplaceOrderRequestMessage);
                    if (!orderResult.IsValid)
                        return orderResult;

                    if (orderResult.Data == null)
                        return result;

                    // enqueue list 
                    foreach (var order in orderResult.Data)
                    {
                        var orderMessage = new MarketplaceServiceMessage<Order>(configResult.Identity, configResult.Data, order);
                        var saveOrderResult = await this.OrderService.SaveOrder(orderMessage);

                        var orderCommandMessage = new CommandMessage<Order>(configResult.Identity)
                        {
                            CorrelationId = Guid.NewGuid().ToString(),
                            Data = order,
                            Marketplace = this.GetMarketplace(),
                            EventDateTime = DateTimeOffset.Now,     
                        };

                        var enqueueResult = await this.BrokerService.EnqueueCommandAsync<Order>(orderCommandMessage);

                        if (!saveOrderResult.IsValid && !enqueueResult.IsValid)
                        {
                            result.WithErrors(saveOrderResult.Errors);
                            result.WithErrors(enqueueResult.Errors);
                        }
                        else
                        {
                            await this.DequeueOrderFromMarketplaceAsync(orderMessage);
                            await this.NotifyOrderIntegrated(orderMessage);
                        }  
                    }

                    if (!orderRequestMessage.Data.IsDone)
                    {
                        orderRequestMessage.Data.SlideDate();
                        await this.SaveState(new ServiceMessage<OrderRequest>(message.Identity, orderRequestMessage.Data), orderRequestMessage.Data.IsScan);
                    }
                    else
                    {
                        await this.RemoveState(new ServiceMessage<OrderRequest>(message.Identity, orderRequestMessage.Data), orderRequestMessage.Data.IsScan);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to get orders to integrate");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage<ShipmentLabel>> GetMarketplaceShipmentLabelAsync(MarketplaceServiceMessage<string> message)
        {
            #region Code

            var result = new ServiceMessage<ShipmentLabel>(message.Identity);

            if (!message.Identity.IsValidVendorTenantAccountIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {
                var configResult = await this.ConfigurationService.GetConfiguration(message);

                if (!configResult.IsValid)
                {
                    result.WithErrors(configResult.Errors);
                    return result;
                }

                var shipmentLabelResult = await GetShipmentLabelAsync(new MarketplaceServiceMessage<string>(configResult.Identity, configResult.Data, message.Data));

                if (!shipmentLabelResult.IsValid)
                    result.WithErrors(shipmentLabelResult.Errors);

                result.WithData(shipmentLabelResult.Data);

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to get shipment label from order {message.Data}");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual Task<ServiceMessage<ShipmentLabel>> GetMarketplaceShipmentLabelAsync(MarketplaceServiceMessage<IOrderReference>[] message)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<ServiceMessage> SendOrdersToIntegrationAsync(ServiceMessage message)
        {
            #region Code

            var result = ServiceMessage.CreateValidResult(message.Identity);

            if (!message.Identity.IsValidWorkerIdentity())
            {
                result.WithError(new Error("As credenciais são inválidas ou estão ausentes.", "Invalid credentials", ErrorType.Authentication));
                return result;
            }

            try
            {   
                var messageBatches = this.BrokerService.PeekCommand<CommandMessage<Order>>(new CommandMessage<Order>(message.Identity));

                foreach (var batch in messageBatches)
                {
                    var dequeueBatch = new DequeueCommandBatchMessage<Order>();
                    dequeueBatch.Commands = new List<DequeueCommandMessage>();

                    // TODO: CREATE A TASK TO EACH MESSAGE AND USE WHEN ALL PER BATCH TO DEQUEUE
                    foreach(var orderMessage in batch)
                    {
                        var config = await ConfigurationService.GetConfiguration(orderMessage.AsMarketplaceServiceMessage(orderMessage.Command.Identity, GetMarketplace()));

                        // consume
                        var consumeResult = await this.ERPClient.SendOrderAsync(orderMessage.Command.Data.AsMarketplaceServiceMessage(orderMessage.Command.Identity, config.Data));

                        // if succes add to dequeue array
                        if (consumeResult.IsValid)
                            dequeueBatch.Commands.Add(new DequeueCommandMessage() { Marketplace = GetMarketplace().ToString(), MessageId = orderMessage.MessageId, ReceiptHandle = orderMessage.ReceiptHandle });
                    }

                    if (dequeueBatch.Commands.Any())
                    {
                        var dequeueResult = this.BrokerService.DequeueCommandBatchAsync<Order>(dequeueBatch.AsServiceMessage(message.Identity)).GetAwaiter().GetResult();
                        result.WithErrors(dequeueResult.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to send order to integration");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;

            #endregion
        }

        public virtual async Task<ServiceMessage> DequeueOrderAsync(MarketplaceServiceMessage<Order> message)
        {
            // mandatory
            var cacheResult = await this.CacheService.Set<bool>(new ServiceMessage<(string key, bool value, TimeSpan? expires, StackExchange.Redis.When? when)>(message.Identity,($"orderStatus-{message.Data.IntegrationOrderId}-{message.Data.OrderStatus}", true, TimeSpan.FromDays(30), StackExchange.Redis.When.Always)));
            // marketplace optional
            var dequeueResult = await this.DequeueOrderFromMarketplaceAsync(message);

            return cacheResult;
        }

        #region Caching Methods 

        /// <summary>
        /// Loads orderRequest state
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isScan"></param>
        /// <returns></returns>
        private async Task<ServiceMessage<OrderRequest>> LoadState(ServiceMessage message, bool isScan)
        {
            var result = new ServiceMessage<OrderRequest>(message.Identity);
            var key = GetCacheKey(new ServiceMessage<(bool isScan, bool isState)>(message.Identity, (isScan, true)));

            try
            {
                var cacheResult = await this.CacheService.Get<OrderRequest>(new ServiceMessage<string>(message.Identity, key));
                if (!cacheResult.IsValid)
                {
                    result.WithErrors(cacheResult.Errors);
                    return result;
                }

                result.Data = cacheResult.Data;

            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to load search state");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;
        }

        /// <summary>
        /// Loads orderRequest state
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isScan"></param>
        /// <returns></returns>
        private async Task<ServiceMessage> SaveState(ServiceMessage<OrderRequest> message, bool isScan)
        {
            var result = new ServiceMessage<OrderRequest>(message.Identity);
            var key = GetCacheKey(new ServiceMessage<(bool isScan, bool isState)>(message.Identity, (isScan, true)));

            try
            {
                var cacheResult = await this.CacheService.Set<OrderRequest>(new ServiceMessage<(string key, OrderRequest value, TimeSpan? expires, StackExchange.Redis.When? when)>(message.Identity, (key, message.Data, null, null)));
                if (!cacheResult.IsValid)
                {
                    result.WithErrors(cacheResult.Errors);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to save search state");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;
        }

        /// <summary>
        /// Loads orderRequest state
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isScan"></param>
        /// <returns></returns>
        private async Task<ServiceMessage> RemoveState(ServiceMessage<OrderRequest> message, bool isScan)
        {
            var result = new ServiceMessage<OrderRequest>(message.Identity);
            var key = GetCacheKey(new ServiceMessage<(bool isScan, bool isState)>(message.Identity, (isScan, true)));

            try
            {
                var cacheResult = await this.CacheService.Remove(new ServiceMessage<string>(message.Identity, key));
                if (!cacheResult.IsValid)
                {
                    result.WithErrors(cacheResult.Errors);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, $"Error while try to delete search state");
                Logger.LogCustomCritical(error, message.Identity);
                result.WithError(error);
            }

            return result;
        }

        /// <summary>
        /// Retrieves order caching key
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isScan"></param>
        /// <param name="isData"></param>
        /// <returns></returns>
        private static string GetCacheKey(ServiceMessage<(bool isScan, bool isState)> message)
        {
            return $"order:{message.Identity.GetVendorId()}:{message.Identity.GetTenantId()}:{message.Identity.GetAccountId()}:scan={message.Data.isScan}:state={message.Data.isState}";
        }

        #endregion
    }
}