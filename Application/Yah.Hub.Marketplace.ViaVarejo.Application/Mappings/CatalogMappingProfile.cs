using AutoMapper;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog;
using System.Globalization;
using Attribute = Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog.Attribute;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Mappings
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            this.RegisterProductMapping();
        }

        private void RegisterProductMapping()
        {
            #region [Product]
            base.CreateMap<Domain.Catalog.Product, Product>()
                .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => Convert.ToInt32(src.Category.MarketplaceId)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Warranty, opt => opt.MapFrom(src => src.WarrantyTime))
                .ForMember(dest => dest.Attributes, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var attributes = new List<Attribute>();

                        if(src.Attributes != null && src.Attributes.Any())
                            attributes.AddRange(src.Attributes.Select(x => new Attribute()
                            {
                                Id = x.Key,
                                Value = x.Value.ToString()
                            }));

                        return attributes;
                    });
                })
                .ForMember(dest => dest.Skus, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var skus = new List<Sku>();

                        skus.AddRange(src.Skus.Select(x => new Sku()
                        {
                            SellerSkuId = x.Id,
                            Imagens = x.Images.Select(i => i.Url).ToList(),
                            Price = new Price() 
                            { 
                                ListPrice = x.Price.List.ToString(new CultureInfo("pt-BR")).Replace(".", ","),
                                SalePrice = x.Price.SalePrice != null 
                                              ? x.Price.SalePrice?.ToString(new CultureInfo("pt-BR")).Replace(".", ",") 
                                              : x.Price.List.ToString(new CultureInfo("pt-BR")).Replace(".", ",")
                            },
                            Inventory = new Inventory() { Quantity = x.Inventory.Balance, HandlingTime = x.Inventory.HandlingDays ?? 0},
                            Dimension = new Dimension() 
                            { 
                                Depth = decimal.Parse(x.Dimension.Length),
                                Weight = decimal.Parse(x.Dimension.Weight),
                                Width = decimal.Parse(x.Dimension.Width),
                                Height = decimal.Parse(x.Dimension.Height)
                            },
                            Attributes = x.SkuAttributes.Select(x => new Attribute()
                            {
                                Id = x.Key,
                                Value = x.Value.ToString()
                            }).ToList()
                        }));

                        return skus;
                    });
                });
            #endregion

            #region [Price]
            base.CreateMap<Domain.Catalog.Sku, PriceUpdate>()
                .ForMember(dest => dest.SkuId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var price = new Price()
                        {
                            ListPrice = src.Price.List.ToString(new CultureInfo("pt-BR")).Replace(".", ","),
                            SalePrice = src.Price.SalePrice != null
                                              ? src.Price.SalePrice?.ToString(new CultureInfo("pt-BR")).Replace(".", ",")
                                              : src.Price.List.ToString(new CultureInfo("pt-BR")).Replace(".", ",")
                        };

                        return price;
                    });
                });
            #endregion

            #region [Inventory]

            base.CreateMap<Domain.Catalog.Sku, InventoryUpdate>()
                .ForMember(dest => dest.SkuId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Inventory, opt => opt.MapFrom(src => new Inventory() { Quantity = src.Inventory.Balance, HandlingTime = src.Inventory.HandlingDays ?? 0 }));

            #endregion
        }
    }
}
