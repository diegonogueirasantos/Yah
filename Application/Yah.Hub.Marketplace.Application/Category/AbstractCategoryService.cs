using System;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;
using Yah.Hub.Marketplace.Application.Repositories;

namespace Yah.Hub.Marketplace.Application.Category
{
    public abstract class AbstractCategoryService : AbstractMarketplaceService, ICategoryService
    {
        public readonly ICategoryRepository CategoryRepository;
        protected IAccountConfigurationService ConfigurationService { get; }

        public AbstractCategoryService(IConfiguration configuration, ILogger<AbstractCategoryService> logger, ICategoryRepository categoryRepository, IAccountConfigurationService accountConfigurationService) : base(configuration, logger)
        {
            CategoryRepository = categoryRepository;
            ConfigurationService = accountConfigurationService;
        }

        public abstract Task<MarketplaceServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>> GetMarketplaceCategories(MarketplaceServiceMessage serviceMessage);

        public virtual async Task<MarketplaceServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>> GetMarketplaceCategory(MarketplaceServiceMessage<string?> serviceMessage)
        {
            var result = new MarketplaceServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>(serviceMessage.Identity, serviceMessage.AccountConfiguration, new List<MarketplaceCategory>());
            bool isRoot = serviceMessage.Data == null;

            if (!isRoot)
            {
                var categoryResult = await this.CategoryRepository.GetAsync(serviceMessage.Data.AsMarketplaceServiceMessage(serviceMessage.Identity, serviceMessage.AccountConfiguration));
                if (categoryResult.IsValid)
                    result.Data.Add(categoryResult.Data);

                result.WithErrors(categoryResult.Errors);
            }
            else
            {
                var categoryResult = await this.CategoryRepository.GetRootCategories(serviceMessage);
                if(categoryResult.IsValid)
                    result.WithData(categoryResult.Data);

                result.WithErrors(categoryResult.Errors);
            }

            return result;
        }

        public abstract Task<MarketplaceServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage serviceMessage);
        public abstract Task<MarketplaceServiceMessage<List<Yah.Hub.Domain.Attribute.MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage<string?> serviceMessage);

        public virtual async Task<ServiceMessage> ImportCategories (MarketplaceServiceMessage serviceMessage)
        {
            var result = new ServiceMessage(serviceMessage.Identity);

            try
            {
                var configuration = await this.ConfigurationService.GetConfiguration(serviceMessage);
                var getResult = await GetMarketplaceCategories(new MarketplaceServiceMessage<string?>(serviceMessage.Identity, configuration.Data));
                if (!getResult.IsValid)
                    return getResult;

                foreach(var category in getResult.Data)
                {
                    var saveResult = await CategoryRepository.SaveAsync(new MarketplaceServiceMessage<MarketplaceCategory>(serviceMessage.Identity, GetMarketplace(), category));
                    if (!saveResult.IsValid)
                        return saveResult;
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while import categories");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public virtual async Task<ServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>> GetCategory(MarketplaceServiceMessage<string?> serviceMessage)
        {
            var result = new ServiceMessage<List<MarketplaceCategory>>(serviceMessage.Identity);

            try
            {
                var configuration = await this.ConfigurationService.GetConfiguration(serviceMessage);

                var getResult = await GetMarketplaceCategory(new MarketplaceServiceMessage<string?>(serviceMessage.Identity, configuration.Data,serviceMessage.Data));
                if (!getResult.IsValid)
                    result.WithErrors(getResult.Errors);

                result.WithData(getResult.Data);
                return result;
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get categories");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }

        public async Task<ServiceMessage<List<MarketplaceAttributes>>> GetAttribute(MarketplaceServiceMessage<string?> serviceMessage)
        {
            var result = new ServiceMessage<List<MarketplaceAttributes>>(serviceMessage.Identity);

            try
            {
                var configuration = await this.ConfigurationService.GetConfiguration(serviceMessage);

                var getResult = await GetMarketplaceAttribute(new MarketplaceServiceMessage<string?>(serviceMessage.Identity, configuration.Data, serviceMessage.Data));
                if (!getResult.IsValid)
                    result.WithErrors(getResult.Errors);

                result.WithData(getResult.Data);
                return result;
            }
            catch (Exception ex)
            {
                Error error = new Error(ex, "Error while get attributes");
                Logger.LogCustomCritical(error, serviceMessage.Identity);
                result.WithError(error);
            }

            return result;
        }
    }
}

