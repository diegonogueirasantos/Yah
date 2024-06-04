using System;
using AutoMapper;
using Yah.Hub.Domain.Category;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Category;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            this.RegisterMeliCategoryToMarketplaceCategory();
            this.RegisterMeliCategoryAttributeToMarketplaceCategoryAttribute();
        }

        private void RegisterMeliCategoryToMarketplaceCategory()
        {
            base.CreateMap<MeliCategory, MarketplaceCategory>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.HasChildren, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.children_categories?.Any() ?? false;
                    });
                })
                .ForMember(dest => dest.Childrens, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.children_categories?.Select(x => new MarketplaceChildrenCategory()
                        {
                            Id = x.id,
                            Name = x.name
                        });
                    });
                })
                .ForMember(dest => dest.Path, opt =>
                {
                    opt.PreCondition((src) => (src.path_from_root != null && src.path_from_root.Any()));
                    opt.ResolveUsing((src) =>
                    {
                        return src.path_from_root?.Select(x => new MarketplaceCategoryPath()
                        {
                            Id = x.id,
                            Name = x.name
                        }).ToList();
                    });
                });

        }

        private void RegisterMeliCategoryAttributeToMarketplaceCategoryAttribute()
        {
            base.CreateMap<MeliCategoryAttribute, MarketplaceCategoryAttribute>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.value_type))
                .ForMember(dest => dest.Hint, opt => opt.MapFrom(src => src.hint))
                .ForMember(dest => dest.Tooltip, opt => opt.MapFrom(src => src.tooltip))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.IsMandatory, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.tags?.required ?? false;

                    });
                })
                .ForMember(dest => dest.AllowVariations, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.tags?.variation_attribute ?? false;

                    });
                })
                .ForMember(dest => dest.Values, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.values?.Select(x => new MarketplaceCategoryAttributeValue()
                        {
                            Id = x.id,
                            Name = x.name
                        });
                    });
                }).ForMember(dest => dest.AllowedUnits, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.allowed_units?.Select(x => new MarketplaceCategoryAttributeValue()
                        {
                            Id = x.id,
                            Name = x.name
                        });
                    });
                });
        }
    }
}

