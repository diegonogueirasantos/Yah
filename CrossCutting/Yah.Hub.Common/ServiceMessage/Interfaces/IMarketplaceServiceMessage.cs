using System;
using Yah.Hub.Common.ServiceMessage.Interfaces;

namespace Yah.Hub.Common.Marketplace.Interfaces
{
    public interface IMarketplaceServiceMessage : IServiceMessage
    {
        public MarketplaceAlias Marketplace { get; }
    }

    public interface IMarketplaceServiceMessage<T> : IMarketplaceServiceMessage
    {
        public T Data { get; set; }
        public void WithData(T data);
    }
}

