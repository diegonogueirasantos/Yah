using System;
using AutoMapper;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Common.Services;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.Application.Repositories;
using Yah.Hub.Marketplace.MercadoLivre.Application.Client;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Category
{
    public class MercadoLivreCategoryService : AbstractCategoryService, ICategoryService
    {
        private readonly IMercadoLivreClient Client;

        public MercadoLivreCategoryService(
            IConfiguration configuration,
            ILogger<MercadoLivreCategoryService> logger,
            ICategoryRepository categoryRepository,
            IMercadoLivreClient client,
            IAccountConfigurationService accountConfigurationService) 
            : base(configuration, logger, categoryRepository, accountConfigurationService)
        {
            this.Client = client;
        }

        public override async Task<MarketplaceServiceMessage<List<Yah.Hub.Domain.Category.MarketplaceCategory>>> GetMarketplaceCategory(MarketplaceServiceMessage<string?> serviceMessage)
        {
            #region Code

            var result = new MarketplaceServiceMessage<List<MarketplaceCategory>>(serviceMessage.Identity, serviceMessage.AccountConfiguration);
            var isSingle = serviceMessage.Data != null;


            // get category
            var categoryResult = await this.Client.GetCategory(serviceMessage);

            if (!categoryResult.IsValid)
                result.WithErrors(categoryResult.Errors);

            var mappResult = Mapper.Map<List<MarketplaceCategory>>(categoryResult.Data);

            // get category attributes
            if (isSingle)
            {
                var attributeResult = await this.Client.GetCategoryAttributes(serviceMessage);

                if (attributeResult.IsValid)
                    mappResult.FirstOrDefault().Attributes = Mapper.Map<List<MarketplaceCategoryAttribute>>(attributeResult.Data);
                else
                    result.WithErrors(attributeResult.Errors);
            }

            result.WithData(mappResult);

            return result;

            #endregion
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.MercadoLivre;
        }

        public override Task<MarketplaceServiceMessage<List<MarketplaceCategory>>> GetMarketplaceCategories(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        public override Task<MarketplaceServiceMessage<List<MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        public override Task<MarketplaceServiceMessage<List<MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage<string?> serviceMessage)
        {
            throw new NotImplementedException();
        }
    }
}

