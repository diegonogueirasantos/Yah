using AutoMapper;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Mappings
{
    public class SaleMappingProfile : Profile
    {
        public SaleMappingProfile()
        {
            this.RegisterOrderMapping();
            this.RegisterOrderStatusMapping();
        }

        public void RegisterOrderMapping()
        {
            base.CreateMap<Order, Domain.Order.Order>()
                .ConstructUsing(src => new Domain.Order.Order(src.Id))
                .ForMember(dest => dest.IntegrationOrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Marketplace, opt => opt.UseValue(MarketplaceAlias.ViaVarejo.ToString()))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchasedAt))
                .ForMember(dest => dest.MarketplaceBrand, opt => opt.MapFrom(src => src.Site))
                .ForMember(dest => dest.BillingAddress, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.ShippingAddress, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Logistic, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Customer, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.OrderStatus, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Payment, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Items, opt => opt.ResolveUsing<OrderResolver>());
        }

        public void RegisterOrderStatusMapping()
        {
            base.CreateMap<OrderStatusInvoice, InvoiceOrder>()
                .ForMember(dest => dest.Items, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var order = ctx.Items[MappingContextKeys.Order] as Domain.Order.Order;

                        var items = order.Items.GroupBy(x => x.Id).Select(x => $"{x.Key}-{x.Count()}").ToArray();
                        
                        return items;
                    });
                })
                .ForMember(dest => dest.OccurredAt, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {    
                        return src.Date?.ToString() ?? DateTime.Now.ToString();
                    });
                })
                .ForMember(dest => dest.Invoice, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var invoice = new Invoice()
                        {
                            Cnpj = src.Cnpj,
                            Number = src.Number,
                            Serie = src.Series,
                            IssuedAt = src.Date?.ToString() ?? DateTime.Now.ToString(),
                            AccessKey = src.Key,
                            LinkXml = src.Url,
                        };

                        return invoice;
                    });
                });

            base.CreateMap<OrderStatusShipment, OrderShipment>()
                .ForMember(dest => dest.Items, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var order = ctx.Items[MappingContextKeys.Order] as Domain.Order.Order;

                        var items = order.Items.GroupBy(x => x.Id).Select(x => $"{x.Key}-{x.Count()}").ToArray();

                        return items;
                    });
                })
                .ForMember(dest => dest.OccurredAt, opt => opt.UseValue(DateTime.Now.ToString()))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.TrackingUrl))
                .ForMember(dest => dest.TrackingCode, opt => opt.MapFrom(src => src.TrackingCode))
                .ForMember(dest => dest.SellerDeliveryId, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var order = ctx.Items[MappingContextKeys.Order] as Domain.Order.Order;

                        return order.IntegrationOrderId;
                    });
                })
                .ForMember(dest => dest.Cte, opt => opt.MapFrom(src => src.Cte))
                .ForMember(dest => dest.Carrier, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, ctx) =>
                    {
                        var carrier = new Carrier() { Cnpj = src.Cnpj, Name = src.CarrierName };

                        return carrier;
                    });
                });
        }
    }
}
