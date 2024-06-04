using AutoMapper;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Netshoes.Application.Models;
using System.Linq.Expressions;
using Attribute = Yah.Hub.Marketplace.Netshoes.Application.Models.Attribute;

namespace Yah.Hub.Marketplace.Netshoes.Application.Mappings
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            this.RegisterProductMapping();
        }

        public void RegisterProductMapping() 
        {
            base.CreateMap<Domain.Catalog.Sku, Product>()
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Variations.Where(x => x.Id.ToLower() == "color").Select(x => x.ValueId ?? x.Value).FirstOrDefault()))
                .ForMember(dest => dest.Flavor, opt => opt.MapFrom(src => src.Variations.Where(x => x.Id.ToLower() == "flavor").Select(x => x.ValueId ?? x.Value).FirstOrDefault()))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Variations.Where(x => x.Id.ToLower() == "size").Select(x => x.ValueId ?? x.Value).FirstOrDefault()))
                .ForMember(dest => dest.EanIsbn, opt => opt.MapFrom(src => src.SkuAttributes.Where(x => x.Key == "ean").Select(x => x.Value).FirstOrDefault().ToString()))
                .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Weight)))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Width)))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Height)))
                .ForMember(dest => dest.Depth, opt => opt.MapFrom(src => decimal.Parse(src.Dimension.Length)))
                .ForMember(dest => dest.Images, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var netshoesImages = new List<Image>();

                        if (src.Images != null && src.Images.Any())
                            netshoesImages.AddRange(src.Images.Select(x => new Image() { Url = x.Url }).ToList());

                        return netshoesImages;
                    });
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var netshoesPrice = new Price()
                        {
                            SalePrice = src.Price.SalePrice ?? src.Price.List,
                            ListPrice = src.Price.List
                        };

                        return netshoesPrice;
                    });
                })
                .ForMember(dest => dest.Stock, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var netshoesStock = new Stock()
                        {
                            Available = src.Inventory.Balance
                        };

                        return netshoesStock;
                    });
                })
                .AfterMap((src, dest, ctx) =>
                {
                    var product = ctx.Items[MappingContextKeys.Product] as Domain.Catalog.Product;

                    dest.ProductGroup = product.Id;
                    dest.Name = product.Name;
                    dest.Description = product.Description;
                    dest.Brand = product.Brand;
                    dest.Department = product.Category.MarketplaceId.Split("-").First();
                    dest.ProductType = product.Category.MarketplaceId.Split("-").Last();

                    if (product.Attributes != null)
                    {
                        dest.Gender = product.Attributes.Where(x => x.Key == "gender").Select(x => x.Value).FirstOrDefault().ToString();

                        if (String.IsNullOrEmpty(dest.Gender))
                            dest.Gender = "Unissex";

                        if(String.IsNullOrEmpty(dest.Size))
                            dest.Size = "Único";

                        dest.Attributes.AddRange(product.Attributes.Select(x => new Attribute()
                        {
                            Name = x.Key,
                            Values = new List<string>() { x.Value.ToString() }
                        }));
                    }

                    if(src.SkuAttributes != null)
                        dest.Attributes.AddRange(src.SkuAttributes.Select(x => new Attribute()
                        {
                           Name =  x.Key,
                           Values = new List<string>() { x.Value.ToString()}
                        }));
                });
                
        }
    }
}
