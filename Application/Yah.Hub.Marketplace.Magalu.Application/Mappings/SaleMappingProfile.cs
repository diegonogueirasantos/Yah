using AutoMapper;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Marketplace.Magalu.Application.Mappings.Resolvers;
using Yah.Hub.Marketplace.Magalu.Application.Models;
using Yah.Hub.Marketplace.Magalu.Application.Models.Order;

namespace Yah.Hub.Marketplace.Magalu.Application.Mappings
{
    public class SaleMappingProfile : Profile
    {
        public SaleMappingProfile()
        {
            this.RegisterOrderMapping();
            this.RegisterOrderStatusMapping();
        }

        private void RegisterOrderMapping()
        {
            base.CreateMap<MagaluOrder, Domain.Order.Order>()
                .ConstructUsing(src => new Order(src.IdOrder))
                .ForMember(dest => dest.IntegrationOrderId, opt => opt.MapFrom(src => src.IdOrder))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchasedDate))
                .ForMember(dest => dest.Marketplace, opt => opt.MapFrom(src => MarketplaceAlias.Magalu.ToString()))
                .ForMember(dest => dest.Logistic, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.BillingAddress, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.ShippingAddress, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Customer, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.OrderStatus, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Payment, opt => opt.ResolveUsing<OrderResolver>())
                .ForMember(dest => dest.Items, opt => opt.ResolveUsing<OrderResolver>());
        }

        public void RegisterOrderStatusMapping()
        {
            this.CreateMap<OrderStatusInvoice, MagaluInvoiceOrder>()
                .ForMember(x => x.IdOrder, opt => opt.MapFrom(x => x.IntegrationOrderId))
                .ForMember(x => x.Status, opt => opt.UseValue("INVOICED"))
                .ForMember(x => x.InvoicedNumber, opt => opt.MapFrom(x => x.Number))
                .ForMember(x => x.InvoicedLine, opt => opt.MapFrom(x => x.Series))
                .ForMember(x => x.InvoicedKey, opt => opt.MapFrom(x => x.Key))
                .ForMember(x => x.InvoicedIssueDate, opt => opt.MapFrom(x => x.Date ?? DateTime.Now));

            this.CreateMap<OrderStatusShipment, MagaluShipOrder>()
                .ForMember(x => x.IdOrder, opt => opt.MapFrom(x => x.IntegrationOrderId))
                .ForMember(x => x.Status, opt => opt.UseValue("SHIPPED"))
                .ForMember(x => x.ShippedEstimatedDelivery, opt => opt.MapFrom(x => x.EstimatedTimeArrival))
                .ForMember(x => x.ShippedCarrierDate, opt => opt.ResolveUsing(x => x.Date ?? DateTime.Now))
                .ForMember(x => x.ShippedCarrierName, opt => opt.MapFrom(x => !string.IsNullOrWhiteSpace(x.CarrierName) ? x.CarrierName : "Transportadora"))
                .ForMember(x => x.ShippedTrackingProtocol, opt => opt.MapFrom(x => x.TrackingCode))
                .ForMember(x => x.ShippedTrackingUrl, opt => opt.MapFrom(x => x.TrackingUrl));

            base.CreateMap<OrderStatusDelivered, MagaluDeliveryOrder>()
                .ForMember(x => x.IdOrder, opt => opt.MapFrom(x => x.IntegrationOrderId))
                .ForMember(x => x.Status, opt => opt.UseValue("DELIVERED"))
                .ForMember(x => x.DeliveredDate, opt => opt.ResolveUsing(src => src.DeliveryDate ?? DateTime.Now));

            base.CreateMap<Order, MagaluOrderStatus>()
                .ForMember(x => x.IdOrder, opt => opt.MapFrom(x => x.IntegrationOrderId))
                .ForMember(x => x.Status, opt => opt.UseValue("PROCESSING"));
        }
    }
}