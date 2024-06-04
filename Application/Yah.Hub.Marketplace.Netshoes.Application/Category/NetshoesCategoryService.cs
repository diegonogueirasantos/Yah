using Yah.Hub.Application.Services.AccountConfigurationService;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Attribute;
using Yah.Hub.Domain.Category;
using Yah.Hub.Marketplace.Application.Category;
using Yah.Hub.Marketplace.Application.Repositories;
using Yah.Hub.Marketplace.Netshoes.Application.Client;

namespace Yah.Hub.Marketplace.Netshoes.Application.Category
{
    public class NetshoesCategoryService : AbstractCategoryService, ICategoryService
    {
        private INetshoesClient Client { get; }

        public NetshoesCategoryService(
            IConfiguration configuration,
            ILogger<AbstractCategoryService> logger,
            ICategoryRepository categoryRepository,
            INetshoesClient netshoesClient,
            IAccountConfigurationService accountConfigurationService) 
            : base(configuration, logger, categoryRepository, accountConfigurationService)
        {
            Client = netshoesClient;
        }

        public override MarketplaceAlias GetMarketplace()
        {
            return MarketplaceAlias.Netshoes;
        }

        public async override Task<MarketplaceServiceMessage<List<MarketplaceCategory>>> GetMarketplaceCategories(MarketplaceServiceMessage serviceMessage)
        {
            #region [Code]
            var result = new MarketplaceServiceMessage<List<MarketplaceCategory>>(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            var categoriesResult = new List<MarketplaceCategory>();

            var rootCategories = await this.Client.GetCategories(new MarketplaceServiceMessage<string>(serviceMessage.Identity, serviceMessage.AccountConfiguration, null));

            if (!rootCategories.IsValid)
            {
                if (rootCategories.Errors.Any())
                {
                    result.WithErrors(rootCategories.Errors);
                    return result;
                }
                else
                {
                    result.WithError(new Common.ServiceMessage.Error($"Erro ao tentar obter as categorias do marketplace {serviceMessage.Marketplace}", "erro desconhecido", Common.ServiceMessage.ErrorType.Technical));
                }
            }

            foreach (var category in rootCategories.Data.Categories)
            {
                var mainCategory = new MarketplaceCategory(category.Code)
                {
                    Name = category.Name,
                    Path = new List<MarketplaceCategoryPath>() { new MarketplaceCategoryPath() { Name = category.Name, Id = category.Code } }
                };

                var childCategories = await this.Client.GetCategories(category.Code.AsMarketplaceServiceMessage(serviceMessage));

                if (childCategories.IsValid)
                {
                    if (childCategories.Data != null && childCategories.Data.Categories.Any())
                    {
                        mainCategory.HasChildren = true;

                        foreach (var child in childCategories.Data.Categories)
                        {
                            var childName = $"{category.Name}-{child.Name}";
                            var childCode = $"{category.Code}-{child.Code}";

                            var attributesResult = await this.Client.GetAttributes(new MarketplaceServiceMessage<(string categoryId, string attributeId)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (category.Code, child.Code)));

                            if (attributesResult.IsValid)
                            {
                                if (attributesResult.Data != null && attributesResult.Data.Attributes.Any())
                                {
                                    var childCategory = new MarketplaceCategory(childCode)
                                    {
                                        Name = childName,
                                        Path = new List<MarketplaceCategoryPath>() { new MarketplaceCategoryPath() { Name = category.Name , Id = category.Code }, new MarketplaceCategoryPath() { Name = childName, Id = childCode } },
                                        ParentId = category.Name,
                                        HasChildren = false,
                                        Attributes = attributesResult.Data.Attributes.Select(a => new MarketplaceCategoryAttribute()
                                        {
                                            Id = a.Id,
                                            Name = a.Name,
                                            IsMandatory = a.IsRequired,
                                            Type = a.TypeSelection,
                                            Values = a.Values?.Select(v => new MarketplaceCategoryAttributeValue()
                                            {
                                                Id = v.Id,
                                                Name = v.Value
                                            }).ToList()
                                        }).ToList()
                                    };

                                    mainCategory.Childrens.Add(new MarketplaceChildrenCategory() { Id = childCode, Name = childName });
                                    categoriesResult.Add(childCategory);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (attributesResult.Errors != null && attributesResult.Errors.Any())
                                {
                                    result.WithErrors(attributesResult.Errors);
                                }
                                else
                                {
                                    result.WithError(new Error($"Erro ao tentar obter as informações de atributo da categoria ID {child.Code}", $"erro desconhecido, statuscode: {childCategories.StatusCode}", ErrorType.Technical));
                                }
                            }
                        }

                    }
                    categoriesResult.Add(mainCategory);

                }
                else
                {
                    if (childCategories.Errors != null && childCategories.Errors.Any())
                    {
                        result.WithErrors(childCategories.Errors);
                    }
                    else
                    {
                        result.WithError(new Error($"Erro ao tentar obter as informações da categoria ID {category.Code}", $"erro desconhecido, statuscode: {childCategories.StatusCode}", ErrorType.Technical));
                    }
                }

            }

            result.WithData(categoriesResult);

            return result;
            #endregion

        }

        //public async override Task<MarketplaceServiceMessage<List<MarketplaceCategory>>> GetMarketplaceCategory(MarketplaceServiceMessage<string?> serviceMessage)
        //{
        //    #region [Code]
        //    var result = new MarketplaceServiceMessage<List<MarketplaceCategory>>(serviceMessage.Identity,serviceMessage.AccountConfiguration);

        //    var categoriesResult = new List<MarketplaceCategory>();

        //    var isSingle = serviceMessage.Data != null;

        //    var rootCategories = await this.Client.GetCategories(serviceMessage);

        //    if (!rootCategories.IsValid)
        //    {
        //        if (rootCategories.Errors.Any())
        //        {
        //            result.WithErrors(rootCategories.Errors);
        //            return result;
        //        }
        //        else
        //        {
        //            result.WithError(new Common.ServiceMessage.Error($"Erro ao tentar obter as categorias do marketplace {serviceMessage.Marketplace}", "erro desconhecido", Common.ServiceMessage.ErrorType.Technical));
        //        }
        //    }

        //    foreach (var category in rootCategories.Data.Categories)
        //    {
        //        var mainCategory = new MarketplaceCategory(category.Code)
        //        {
        //            Name = category.Name,
        //            Path = new List<MarketplaceCategoryPath>() { new MarketplaceCategoryPath() { Name = category.Name } }
        //        };

        //        var childCategories = await this.Client.GetCategories(category.Code.AsMarketplaceServiceMessage(serviceMessage));

        //        if (childCategories.IsValid)
        //        {
        //            if(childCategories.Data != null && childCategories.Data.Categories.Any())
        //            {
        //                mainCategory.HasChildren = true;

        //                if (!isSingle)
        //                {
        //                    mainCategory.Childrens = childCategories.Data.Categories.Select(x => new MarketplaceChildrenCategory() { Id = x.Code, Name = x.Name }).ToList();
        //                }
        //                else
        //                {
        //                    foreach (var child in childCategories.Data.Categories)
        //                    {
        //                        var attributesResult = await this.Client.GetAttributes(new MarketplaceServiceMessage<(string categoryId, string attributeId)>(serviceMessage.Identity, serviceMessage.AccountConfiguration, (category.Code, child.Code)));

        //                        if (attributesResult.IsValid)
        //                        {
        //                            if(attributesResult.Data != null && attributesResult.Data.Attributes.Any())
        //                            {
        //                                var childCategory = new MarketplaceCategory(child.Code)
        //                                {
        //                                    Name = $"{category.Name}-{child.Name}",
        //                                    Path = new List<MarketplaceCategoryPath>() { new MarketplaceCategoryPath() { Name = category.Name } },
        //                                    ParentId = category.Name,
        //                                    HasChildren = false,
        //                                    Attributes = attributesResult.Data.Attributes.Select(a => new MarketplaceCategoryAttribute()
        //                                    {
        //                                        Id = a.Id,
        //                                        Name = a.Name,
        //                                        IsMandatory = a.IsRequired,
        //                                        Type = a.TypeSelection,
        //                                        Values = a.Values?.Select(v => new MarketplaceCategoryAttributeValue()
        //                                        {
        //                                            Id = v.Id,
        //                                            Name = v.Value
        //                                        }).ToList()
        //                                    }).ToList()
        //                                };

        //                                mainCategory.Childrens.Add(new MarketplaceChildrenCategory() { Id = child.Code, Name = child.Name });
        //                                categoriesResult.Add(childCategory);
        //                            }
        //                            else
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (attributesResult.Errors != null && attributesResult.Errors.Any())
        //                            {
        //                                result.WithErrors(attributesResult.Errors);
        //                            }
        //                            else
        //                            {
        //                                result.WithError(new Error($"Erro ao tentar obter as informações de atributo da categoria ID {child.Code}", $"erro desconhecido, statuscode: {childCategories.StatusCode}", ErrorType.Technical));
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            categoriesResult.Add(mainCategory);

        //        }
        //        else
        //        {
        //            if(childCategories.Errors != null && childCategories.Errors.Any())
        //            {
        //                result.WithErrors(childCategories.Errors);
        //            }
        //            else
        //            {
        //                result.WithError(new Error($"Erro ao tentar obter as informações da categoria ID {category.Code}",$"erro desconhecido, statuscode: {childCategories.StatusCode}",ErrorType.Technical));
        //            }
        //        }
                
        //    }

        //    result.WithData(categoriesResult);

        //    return result;
        //    #endregion
        //}

        public override Task<MarketplaceServiceMessage<List<MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage serviceMessage)
        {
            throw new NotImplementedException();
        }

        public async override Task<MarketplaceServiceMessage<List<MarketplaceAttributes>>> GetMarketplaceAttribute(MarketplaceServiceMessage<string?> serviceMessage)
        {
            #region [Code]
            var result = new MarketplaceServiceMessage<List<MarketplaceAttributes>>(serviceMessage.Identity, serviceMessage.AccountConfiguration);

            var attributeList = new List<MarketplaceAttributes>();

            List<string> attributeIds = String.IsNullOrEmpty(serviceMessage.Data) 
                                        ? new List<string>() { "colors", "flavors", "brands", "sizes" } 
                                        : new List<string>() { serviceMessage.Data};

            foreach (var attribute in attributeIds)
            {
                var attributeResult = await this.Client.GetTemplateAttributes(attribute.AsMarketplaceServiceMessage(serviceMessage));

                if (!attributeResult.IsValid)
                {
                    if (attributeResult.Errors.Any())
                    {
                        result.WithErrors(attributeResult.Errors);

                        continue;
                    }
                    else
                    {
                        result.WithError(new Common.ServiceMessage.Error($"Erro ao tentar obter os dados do atributo {attribute}", "erro desconhecido", Common.ServiceMessage.ErrorType.Technical));

                        continue;
                    }
                        
                }

                attributeList.AddRange(attributeResult.Data.Attributes.Select(x => new MarketplaceAttributes()
                {
                    Id = x.Code ?? x.Id,
                    Name = x.Name,
                }));
            }

            result.WithData(attributeList);

            return result;

            #endregion
        }
    }
}
