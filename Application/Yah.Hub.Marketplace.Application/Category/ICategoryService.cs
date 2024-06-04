using System;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;

namespace Yah.Hub.Marketplace.Application.Category
{
    public interface ICategoryService
    {
        public Task<ServiceMessage> ImportCategories(MarketplaceServiceMessage serviceMessage);
        public Task<ServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>> GetCategory(MarketplaceServiceMessage<string?> serviceMessage);
        public Task<ServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetAttribute(MarketplaceServiceMessage<string?> serviceMessage);
    }
}

