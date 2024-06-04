using AutoMapper;
using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.Application.Repositories;
using Yah.Hub.Marketplace.ViaVarejo.Application.Client.Interface;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Category;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Category
{
    public class ViaVarejoCategoryService : AbstractCategoryService, ICategoryService
    {
        private IViaVarejoClient Client { get; }
        public ViaVarejoCategoryService(
            IConfiguration configuration,
            ILogger<AbstractCategoryService> logger,
            ICategoryRepository categoryRepository,
            IViaVarejoClient viaVarejoClient,
            IAccountConfigurationService accountConfigurationService) 
            : base(configuration, logger, categoryRepository, accountConfigurationService)
        {
            Client = viaVarejoClient;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.ViaVarejo;
        }

        public async override Task<MarketplaceServiceMessage<List<MarketplaceCategory>>> GetMarketplaceCategories(MarketplaceServiceMessage serviceMessage)
        {
            return await this.GetMarketplaceCategory(new MarketplaceServiceMessage<string?>(serviceMessage.Identity,serviceMessage.AccountConfiguration));
        }

        public async override Task<MarketplaceServiceMessage<List<MarketplaceCategory>>> GetMarketplaceCategory(MarketplaceServiceMessage<string?> serviceMessage)
        {
            var result = new MarketplaceServiceMessage<List<MarketplaceCategory>>(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            var requestMessage = new SearchCategory(0, 1, serviceMessage.Data);

            var categories = new List<MarketplaceCategory>();

            do
            {
                var requestResult = await this.Client.GetCategories(requestMessage.AsMarketplaceServiceMessage(serviceMessage));

                var mainCategory = new MarketplaceCategory(requestResult.Data.Categories.First().Id) 
                {
                    Name = requestResult.Data.Categories.First().Name,
                    ParentId = requestResult.Data.Categories.First().ParentId,
                    HasChildren = requestResult.Data.Categories.First().Categories?.Any() ?? false,
                    Childrens = requestResult.Data.Categories?.First()?.Categories?.Select(x => new MarketplaceChildrenCategory(x.Id, x.Name)).ToList() ?? new List<MarketplaceChildrenCategory>(),
                    Attributes = (requestResult.Data.Categories?.First()?.Categories.Any() ?? false) 
                                    ? this.MapAttributes(requestResult.Data.Categories.First().Groups)
                                    : new List<MarketplaceCategoryAttribute>()
                };

                categories.Add(mainCategory);

                requestMessage.Offset++;
                requestMessage.Limit++;

            } while (serviceMessage.Data == null && (requestMessage.Offset <= 49 && requestMessage.Limit <= 50));

            result.WithData(categories);

            return result;
        }

        public override Task<MarketplaceServiceMessage<List<MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        public override Task<MarketplaceServiceMessage<List<MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage<string?> serviceMessage)
        {
            throw new NotImplementedException();
        }

        private List<MarketplaceCategoryAttribute> MapAttributes(Group[] groups)
        {
            var attributesResult = new List<MarketplaceCategoryAttribute>();

            foreach (var vvAtribute in groups)
            {
                if (vvAtribute.Attributes != null && vvAtribute.Attributes.Any())
                {
                    attributesResult
                        .AddRange(vvAtribute.Attributes
                        //.Where(x => x.Required == "Y")
                        .Select(x => new MarketplaceCategoryAttribute()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Type = x.Type,
                            IsMandatory = x.Required == "Y",
                            AllowVariations = x.Variant == "Y",
                            Values = x.Value == null
                                        ? new List<MarketplaceCategoryAttributeValue>()
                                        : x.Value.Select(a => new MarketplaceCategoryAttributeValue()
                                        {
                                            Id = a.Id,
                                            Name = a.Text
                                        }).ToList()
                        }).ToList());

                    if(!attributesResult.Any(x => x.AllowVariations))
                    {
                        attributesResult.Add(new MarketplaceCategoryAttribute()
                        {
                            Id = "3280",
                            Name = "Informações Adicionais",
                            Type = "TXN",
                            IsMandatory = false,
                            AllowVariations = true
                        });
                    }
                }
            }

            return attributesResult;
        }
    }
}
