using AutoMapper;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Order;

namespace Yah.Hub.Marketplace.Netshoes.Application.Mappings
{
    public class SalesMappingProfile: Profile
    {
        public SalesMappingProfile()
        {
            this.RegisterOrderMapping();
        }

        private void RegisterOrderMapping()
        {
            base.CreateMap<Order, Domain.Order.Order>()
                .ConstructUsing(src => new Domain.Order.Order(src.IntegrationOrderNumber))
                .ForMember(dest => dest.IntegrationOrderId, opt => opt.MapFrom(src => src.IntegrationOrderNumber))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeMilliseconds(src.OrderDate).UtcDateTime))
                .ForMember(dest => dest.MarketplaceBrand, opt => opt.MapFrom(src => src.OriginSite))
                .ForMember(dest => dest.BillingAddress, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.ShippingAddress, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Logistic, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Customer, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.OrderStatus, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Payment, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Items, opt => opt.ResolveUsing<OrderResolver>());
        }
    }
}
