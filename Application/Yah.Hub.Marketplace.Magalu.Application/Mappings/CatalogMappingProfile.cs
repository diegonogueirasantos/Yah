using AutoMapper;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Magalu.Application.Models;
using System.Dynamic;
using Attribute = Yah.Hub.Marketplace.Magalu.Application.Models.Attribute;

namespace Yah.Hub.Marketplace.Magalu.Application.Mappings
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            this.RegisterProductMapping();
        }


        public void RegisterProductMapping()
        {
            #region [Product]
            base.CreateMap<Domain.Catalog.Product, Product>()
                .ForMember(dest => dest.IdProduct, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Active, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        return !ctx.Items.GetValueOrDefault<bool>(MappingContextKeys.DisabledProduct);
                    });
                })
                .ForMember(dest => dest.NbmOrigin, opt => opt.UseValue("0"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.WarrantyTime, opt => opt.MapFrom(src => $"{src.WarrantyTime} {src.WarrantyType.Translate()}"))
                .ForMember(dest => dest.Categories, opt => opt.ResolveUsing(src => 
                {
                    var category = src.Category;

                    var MagaluCategoryList = new List<Category>();

                    var MagaluCategory = new Category()
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name,
                        ParentId = category.ParentId.ToString()
                    };

                    MagaluCategoryList.Add(MagaluCategory);

                    foreach (var c in category.Path)
                    {
                        MagaluCategoryList.Add(new Category() { Id = c.Id.ToString(), Name = c.Name, ParentId = c.ParentId.ToString() });
                    }

                    return MagaluCategoryList;

                }))
                .ForMember(dest => dest.Attributes, opt => opt.ResolveUsing(src =>
                {
                    var magaluAttribute = new List<Attribute>();

                    if (src.Attributes != null)
                    {
                        magaluAttribute.AddRange(src.Attributes.Select(x => new Attribute()
                        {
                            Name = x.Key,
                            Value = x.Value.ToString()
                        }).ToList());
                    };

                    return magaluAttribute;
                }));
            #endregion

            #region [SKU]
            base.CreateMap<Domain.Catalog.Product, List<Sku>>()
                            .ConstructUsing((src, context) => src.Skus
                                    .Select(x => Mapper.Map<Domain.Catalog.Sku, Sku>(x, opt => opt.Items[MappingContextKeys.DisabledProduct] = context.Items[MappingContextKeys.DisabledProduct]))
                                    .Select(x => { x.IdProduct = src.Id; return x; })
                                    .Select(x => { x.Description = src.Description; return x; })
                                    .ToList());

            base.CreateMap<Domain.Catalog.Sku, Sku>()
                .ForMember(dest => dest.IdSku, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IdSkuErp, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        return true;
                    });
                })
                .ForMember(dest => dest.Variation, opt =>
                {
                    opt.PreCondition((src, context) => src.Variations?.Any() ?? false);
                    opt.ResolveUsing((src, dest) =>
                    {
                        return string.Join("-", src.Variations.Select(x => x.Value).ToArray());
                    });
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.PreCondition(src => src.Price != null);
                    opt.ResolveUsing(src => new Price()
                    {
                        ListPrice = src.Price.List,
                        SalePrice = src.Price.Retail,
                    });
                })
                .ForMember(dest => dest.StockQuantity, opt =>
                {
                    opt.PreCondition(src => src.Inventory != null);
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var isDisabled = context.Items.GetValueOrDefault<bool>(MappingContextKeys.DisabledProduct);
                        return isDisabled ? 0 : src.Inventory.Balance;
                    });
                })
                .ForMember(dest => dest.CodeEan, opt =>
                {
                    opt.PreCondition(src => src.SkuAttributes != null && src.SkuAttributes.PropertyExists("ean"));
                    opt.ResolveUsing(src => src.SkuAttributes.Where(x => x.Key == "ean").Select(x => x.Value).FirstOrDefault().ToString()?.PadLeft(13, '0'));
                })
                .ForMember(dest => dest.CodeIsbn, opt =>
                {
                    opt.PreCondition(src => src.SkuAttributes != null && src.SkuAttributes.PropertyExists("isbn"));
                    opt.MapFrom(src => src.SkuAttributes.Where(x => x.Key == "isbn").Select(x => x.Value).FirstOrDefault().ToString());
                })
                .ForMember(dest => dest.UrlImages, opt => opt.MapFrom(src => src.Images.Select(x => x.Url).ToList()))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.Images.Where(x => x.IsMain).Select(x => x.Url).FirstOrDefault() ?? src.Images.FirstOrDefault().Url))
                .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Weight) / 100))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Length) / 100))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Width) / 100))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Height) / 100))
                .ForMember(dest => dest.Attributes, opt =>
                {
                    opt.PreCondition(src => src.Variations?.Any() ?? false);
                    opt.ResolveUsing((src, dest) =>
                    {
                        var attrs = new List<Models.Attribute>();

                        if (src.Variations?.Any() ?? false)
                        {
                            foreach (var variation in src.Variations)
                            {
                                switch (variation.Id)
                                {
                                    case "color":
                                        attrs.Add(new Models.Attribute("COR", variation.Value));
                                        break;

                                    case "size":
                                        attrs.Add(new Models.Attribute("TAMANHO", variation.Value));
                                        break;

                                    case "voltage":
                                        attrs.Add(new Models.Attribute("VOLTAGEM", variation.Value));
                                        break;

                                    case "flavor":
                                        attrs.Add(new Models.Attribute("SABOR", variation.Value));
                                        break;
                                }
                            }
                        }

                        return attrs;
                    });
                });
            #endregion

            #region Inventory

            base.CreateMap<Domain.Catalog.ProductInventory, Stock>()
                .ForMember(dest => dest.Quantity, opt =>
                {
                    opt.PreCondition(src => src.AffectedSku?.Inventory != null);
                    opt.MapFrom(src => src.AffectedSku.Inventory.Balance);
                })
                .ForMember(dest => dest.IdSku, opt =>
                {
                    opt.PreCondition(src => src.AffectedSku?.Inventory != null);
                    opt.MapFrom(src => src.AffectedSku.Id);
                });

            #endregion

            #region Price

            base.CreateMap<Domain.Catalog.ProductPrice, Price>()
                .ForMember(dest => dest.ListPrice, opt =>
                {
                    opt.PreCondition(src => src.AffectedSku?.Price != null);
                    opt.MapFrom(src => src.AffectedSku.Price.List);
                })
                .ForMember(dest => dest.SalePrice, opt =>
                {
                    opt.PreCondition(src => src.AffectedSku?.Price != null);
                    opt.MapFrom(src => src.AffectedSku.Price.Retail);
                })
                .ForMember(dest => dest.IdSku, opt =>
                {
                    opt.PreCondition(src => src.AffectedSku?.Price != null);
                    opt.MapFrom(src => src.AffectedSku.Id);
                });

            #endregion
        }
    }
    
}
