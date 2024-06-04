using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using AutoMapper;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Models;
using System.Dynamic;

namespace Yah.Hub.Marketplace.B2W.Application.Mappings
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            this.RegisterProductMapping();
            this.RegisterVariationMapping();
        }

        #region Product Mapping
        private void RegisterProductMapping()
        {
            #region [Domain.Catalog.Product -> Product]
            base.CreateMap<Domain.Catalog.Product, Product>()
                
                .ForMember(dest => dest.Sku, opt => { opt.MapFrom(src => src.Id); })
                .ForMember(dest => dest.Name, opt => { opt.MapFrom(src => src.Name); })
                .ForMember(dest => dest.ImagesUrls, opt =>
                {
                    opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Images.Select(x => x.Url).ToList());
                })
                .ForMember(dest => dest.Inventory, opt =>
                {
                    opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Inventory.Balance);
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Price.List);
                })
                .ForMember(dest => dest.PromotionalPrice, opt =>
                {
                    opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Price.Retail != src.Skus.First().Price.List ? src.Skus.First().Price.Retail : default(decimal?));
                })
                .ForMember(dest => dest.Brand, opt =>
                {
                    opt.MapFrom(src => src.Brand);
                })
                .ForMember(dest => dest.EAN, opt =>
                {
                    opt.PreCondition((src, context) => !src.HasVariations && src.Attributes != null && (!String.IsNullOrEmpty(src.Attributes.GetPropertyValue("ean"))));
                    opt.MapFrom(src => src.Attributes.GetPropertyValue("ean"));
                })
                .ForMember(dest => dest.NBM, opt =>
                {
                    opt.PreCondition((src, context) => !src.HasVariations && src.Attributes != null && (!String.IsNullOrEmpty(src.Attributes.GetPropertyValue("nbm"))));
                    opt.MapFrom(src => src.Attributes.GetPropertyValue("nbm"));
                })
                .ForMember(dest => dest.Weight, opt =>
                {
                    //opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Dimension.Weight);
                })
                .ForMember(dest => dest.Height, opt =>
                {
                    //opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Dimension.Height);
                })
                .ForMember(dest => dest.Width, opt =>
                {
                    //opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Dimension.Width);
                })
                .ForMember(dest => dest.Length, opt =>
                {
                    //opt.PreCondition((src, context) => !src.HasVariations);
                    opt.MapFrom(src => src.Skus.First().Dimension.Length);
                })
                .ForMember(dest => dest.Status, opt =>
                {
                    opt.PreCondition((src, context) => context.Items.GetValueOrDefault<bool>(MappingContextKeys.DisabledProduct));
                    opt.UseValue<string>("disabled");
                })
                .ForMember(dest => dest.Variations, opt =>
                {
                    opt.PreCondition((src, context) => src.HasVariations);
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var variations = new List<Variation>();

                        foreach(var sku in src.Skus)
                        {
                            variations.Add(ctx.Mapper.Map<Domain.Catalog.Sku, Variation>(sku));
                        }

                        return variations;
                    });
                })
                .ForMember(dest => dest.VariationAttributes, opt =>
                {
                    opt.PreCondition((src, context) => src.HasVariations);
                    opt.MapFrom(src => src.Skus.SelectMany(x => x.Variations).Select(x => x.Id).Distinct().ToList());
                })
                 .ForMember(dest => dest.Categories, opt =>
                 {
                     opt.ResolveUsing((src, ctx) =>
                     {
                         var category = new Category();

                         category.Name = String.Join(" > ", src.Category.Name);
                         category.Code = src.Id;

                         var categoryList = new List<Category>();
                         categoryList.Add(category);
                         return categoryList;
                     });
                 })
                 .ForMember(dest => dest.Description, opt =>
                 {
                     opt.MapFrom(src => src.Description);
                 });
            ;
                
            #endregion

            #region ProductPrice -> Product

            base.CreateMap<Domain.Catalog.ProductPrice, Product>()
                .ForMember(dest => dest.Sku, opt =>
                {
                    opt.MapFrom(src => src.AffectedSku.IntegrationId);
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.MapFrom(src => src.AffectedSku.Price.List);
                })
                .ForMember(dest => dest.PromotionalPrice, opt =>
                {
                    opt.MapFrom(src => src.AffectedSku.Price.Retail);
                });
            #endregion

            #region ProductInventory -> Product
            base.CreateMap<Domain.Catalog.ProductInventory, Product>()
                .ForMember(dest => dest.Sku, opt =>
                {
                    opt.MapFrom(src => src.AffectedSku.IntegrationId);
                })
                .ForMember(dest => dest.Inventory, opt =>
                {
                    opt.MapFrom(src => src.AffectedSku.Inventory.Balance);
                })
                .ForMember(dest => dest.Specifications, opt =>
                {
                    opt.ResolveUsing((src, dest) =>
                    {
                        var specifications = new List<Specification>();

                        var crossDocking = src.AffectedSku.Inventory.HandlingDays?.ToString();

                        if (!String.IsNullOrWhiteSpace(crossDocking))
                        {
                            specifications.Add(new Specification()
                            {
                                Key = "crossDocking",
                                Value = crossDocking
                            });
                        }

                        return specifications.Any() ? specifications : null;
                    });
                });

            #endregion
        }
        #endregion

        #region Variation Mapping

        private void RegisterVariationMapping()
        {
            #region [SKU -> Variation]
            base.CreateMap<Domain.Catalog.Sku, Variation>()
                .ForSourceMember(src => src.Price, opt => opt.Ignore())
                .ForSourceMember(src => src.Inventory, opt => opt.Ignore())
                 .ForMember(dest => dest.Sku, opt =>
                 {
                     opt.MapFrom(src => src.Id);
                 })
                .ForMember(dest => dest.Inventory, opt =>
                {
                    opt.MapFrom(src => src.Inventory.Balance);
                })
                .ForMember(dest => dest.EAN, opt =>
                {
                    opt.PreCondition((src, context) => src.SkuAttributes != null && (!String.IsNullOrEmpty(src.SkuAttributes.GetPropertyValue("ean"))));
                    opt.MapFrom(src => src.SkuAttributes.GetPropertyValue("ean") ?? string.Empty);
                })
                .ForMember(dest => dest.ImagesUrls, opt =>
                {
                    opt.MapFrom(src => src.Images.Select(x => x.Url).ToList());
                })
                .ForMember(dest => dest.Specifications, opt =>
                {
                   opt.PreCondition((src, context) => (src.Variations != null || src.SkuAttributes != null || src.Price != null));
                    opt.ResolveUsing((source, context) =>
                    {
                        #region [Code]
                        List<Specification> specs = new List<Specification>();

                        if (source.Variations != null && source.Variations.Any())
                        {
                            specs.AddRange(source.Variations
                                .Select(attr => new Specification
                                {
                                    Key = attr.Id,
                                    Value = attr.ValueId ?? attr.Value
                                })
                                .ToList()); ;

                            specs.Add(new Specification { Key = "weight", Value = source.Dimension.Weight });
                            specs.Add(new Specification { Key = "height", Value = source.Dimension.Height });
                            specs.Add(new Specification { Key = "width", Value = source.Dimension.Width });
                            specs.Add(new Specification { Key = "length", Value = source.Dimension.Length });
                        }



                        if (source.Price != null)
                        {
                            specs.Add(new Specification
                            {
                                Key = "price",
                                Value = source.Price.List.ToString().Replace(",", ".")
                            });

                            
                            if (source.Price.Retail != source.Price.List)
                            {
                                var promoSpec = new Specification
                                {
                                    Key = "promotional_price"
                                };
                                promoSpec.Value = source.Price.Retail.ToString().Replace(",", ".");
                                specs.Add(promoSpec);
                            }
                        }

                        var handlingDays = source.Inventory?.HandlingDays?.ToString();
                        if (!string.IsNullOrWhiteSpace(handlingDays))
                            specs.Add(new Specification()
                            {
                                Key = "crossDocking",
                                Value = handlingDays,
                            });

                        // attributes
                        if(source.SkuAttributes != null)
                            foreach (var attr in source.SkuAttributes)
                                specs.Add(new Specification { Key = attr.Key, Value = attr.Value?.ToString() ?? "" });

                        if (specs.Any())
                            return specs;
                        else
                            return null;
                        #endregion
                    });
                });
            #endregion

            #region [SkuInventory -> Variation]
            base.CreateMap<Domain.Catalog.SkuInventory, Variation>()
                .ForMember(dest => dest.Sku, opt =>
                {
                    opt.MapFrom(src => src.Id);
                })
                .ForMember(dest => dest.Inventory, opt =>
                {
                    opt.MapFrom(src => src.Inventory.Balance);
                })
                .ForMember(dest => dest.Specifications, opt =>
                {
                    opt.ResolveUsing((src, dest) =>
                    {
                        var specifications = new List<Specification>();

                        var crossDocking = src.Inventory.HandlingDays?.ToString();

                        if (!String.IsNullOrWhiteSpace(crossDocking))
                        {
                            specifications.Add(new Specification()
                            {
                                Key = "crossDocking",
                                Value = crossDocking
                            });
                        }

                        return specifications.Any() ? specifications : null;
                    });
                });
                //.ForAllMembers(opt => opt.Ignore());
            #endregion

            #region [SkuPrice -> Variation]
            base.CreateMap<Domain.Catalog.SkuPrice, Variation>()
                 .ForMember(dest => dest.Sku, opt =>
                 {
                     opt.MapFrom(src => src.Id);
                 })
                .ForMember(dest => dest.Specifications, opt =>
                {
                    opt.ResolveUsing((src, dest) =>
                    {
                        var salePrice = src.Price.Retail != src.Price.List ? src.Price.Retail : src.Price.List;

                        return new List<Specification>
                        {
                            new Specification { Key = "price", Value = src.Price.List.ToString() },
                            new Specification { Key = "promotional_price", Value = salePrice.ToString() }
                        };
                    });
                });
            #endregion

        }

        #endregion


    }
}
