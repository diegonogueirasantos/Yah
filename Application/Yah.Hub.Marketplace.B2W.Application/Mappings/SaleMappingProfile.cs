using System;
using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Yah.Hub.Common.Enums;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Marketplace.B2W.Application.Models;
using Yah.Hub.Marketplace.B2W.Application.Models.Order;

namespace Yah.Hub.Marketplace.B2W.Application.Mappings
{
    public class SaleMappingProfile : Profile
    {
        public SaleMappingProfile()
        {
            this.RegisterB2WOrderToOrder();
            this.RegisterOrderStatusShippedToB2WShipped();
            this.RegisterOrderStatusInvoiceToB2Winvoice();
            this.RegisterOrderStatusDeliveredToB2WDelivered();
            this.RegisterOrderStatusCancelledToB2WCancelled();
            this.RegisterOrderStatusShipExceptionB2WDelivered();
        }


        private void RegisterB2WOrderToOrder()
        {
            base.CreateMap<B2WOrder, Order>()
                .ConstructUsing(src => new Order(src.code))
                .ForMember(dest => dest.IntegrationOrderId, opt => opt.MapFrom(src => src.code))
                .ForMember(dest => dest.Marketplace, opt => opt.UseValue("bw2"))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.placed_at))
                .ForMember(dest => dest.Items, opt =>
                {
                    #region [Code]
                    opt.ResolveUsing((src) =>
                    {
                        return src.items.Select(x =>
                        {
                            return new Domain.Order.Item()
                            {
                                Id = x.id,
                                ExternalId = x.product_id,
                                Quantity = x.qty,
                                Price = (decimal)x.original_price,
                                SalesPrice = (decimal)x.special_price ,
                                Name = x.name,
                                ReferenceId = x.product_id
                            };
                        }).ToList();
                    });
                    #endregion
                })
                .ForMember(dest => dest.Customer, opt =>
                {
                    #region [Code]

                    opt.ResolveUsing((src) =>
                    {
                        return new Domain.Order.Customer()
                        {
                            Type =  Common.Enums.CustomerType.Person, // B2W não informa tipo de comprador
                            Name = src.customer?.name,
                            Surname = String.Empty, // B2W não informa sobrenome
                            Email = src.customer.email,
                            Gender = src.customer.gender == "female" ? Common.Enums.Gender.Female : Common.Enums.Gender.Male, 
                            DocumentNumber = src.customer.vat_number,
                            Phone = string.Join("|", src.customer.phones),
                            //Cellphone = string.Join("|", src.customer.phones),//B2W  só informa o campo phone
                            TradingName =  String.Empty,
                            StateInscription = src.customer.vat_number
                        };
                    });

                    #endregion
                })
                .ForMember(dest => dest.ShippingAddress, opt =>
                {
                    #region [Code]
                    opt.ResolveUsing((src) =>
                    {
                        return new Domain.Order.Address()
                        {
                            AddressLine = src.shipping_address.street,
                            Number = src.shipping_address.number,
                            PostalCode = src.shipping_address.postcode,
                            Neighbourhood = src.shipping_address.neighborhood ?? "Bairro não informado",
                            City = src.shipping_address.city,
                            State = src.shipping_address.region,
                            AddressNotes = src.shipping_address.complement,
                            FullName = src.shipping_address.full_name
                        };
                    });
                    #endregion
                })
                .ForMember(dest => dest.BillingAddress, opt =>
                {
                    #region[Code]
                    opt.PreCondition(src => src.billing_address != null);
                    opt.ResolveUsing((src) =>
                    {
                        return new Domain.Order.Address()
                        {
                            AddressLine = src.billing_address.street,
                            Number = src.billing_address.number,
                            PostalCode = src.billing_address.postcode,
                            Neighbourhood = src.billing_address.neighborhood ?? "Bairro não informado",
                            City = src.billing_address.city,
                            AddressNotes = src.billing_address.complement,
                            FullName = src.billing_address.full_name
                        }
                        .StateNameToUF(src.billing_address.region);
                    });
                    #endregion
                })
                .ForMember(dest => dest.Logistic, opt =>
                {
                    #region [Code]
                    opt.ResolveUsing((src) =>
                    {
                        var logisticType = src.calculation_type != null && src.calculation_type.Equals("b2wentrega")
                                                ? src.ShippingMethod != null && src.ShippingMethod.Equals("B2W Fulfillment")
                                                    ? DeliveryLogisticType.MarketplaceFulfillment
                                                    : DeliveryLogisticType.MarketplaceDelivery
                                                : DeliveryLogisticType.SellerFulfillment;

                        return new Domain.Order.Logistic()
                        {
                            TotalAmount = (decimal)src.shipping_cost,
                            ETA = Convert.ToInt16(new DateTimeOffset(src.estimated_delivery).Subtract(DateTimeOffset.Now).TotalDays), // VERI
                            DeliveryLogisticType = logisticType,
                            TrackingCode = src.shipping_estimate_id, // TODO: VERIFY THIS FIELD
                            CarrierName = src.shipping_carrier,
                            CarrierId = 0
                        };
                    });
                    #endregion
                })
                // https://desenvolvedores.skyhub.com.br/pedidos/status-de-pedidos
                .ForMember(dest => dest.OrderStatus, opt =>
                {
                    #region [Code]

                    opt.ResolveUsing((src) =>
                    {
                        switch (src.status.type)
                        {
                            case "NEW":
                                return OrderStatusEnum.WaitingPayment;
                            case "APPROVED":
                                return OrderStatusEnum.Paid;
                            case "ORDER_INVOICED":
                                return OrderStatusEnum.Invoiced;
                            case "ORDER_SHIPPED":
                                return OrderStatusEnum.Shipped;
                            case "COMPLETE":
                                return OrderStatusEnum.Delivered;
                            case "CANCELLED":
                                return OrderStatusEnum.Canceled;
                            default:
                                return OrderStatusEnum.WaitingPayment;
                        }
                    });

                    #endregion
                });
        }

        private void RegisterOrderStatusInvoiceToB2Winvoice()
        {
            base.CreateMap<OrderStatusInvoice, B2WInvoiceOrder>()
                .ForMember(dest => dest.status, opt => opt.UseValue("order_invoiced"))
                .ForMember(dest => dest.invoice, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return new B2WInvoice()
                        {
                             key = src.Key
                        };
                    });
                });

        }

        private void RegisterOrderStatusShippedToB2WShipped()
        {
            base.CreateMap<OrderStatusShipment, B2WShipOrder>()
                .ForMember(dest => dest.status, opt => opt.UseValue("order_shipped"))
                .ForMember(dest => dest.shipment, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var order = (Order)context.Items[MappingContextKeys.Order];
                        var shipment = new B2WShipment()
                        {
                            code = order.IntegrationOrderId,
                            items = order.Items.Select(x => new B2WItem()
                            {
                                sku = x.Id,
                                qty = x.Quantity
                            }).ToList(),
                            track = new B2WTrack()
                            {
                                code = src.TrackingCode,
                                carrier = order.Logistic.CarrierName,
                                url = src.TrackingUrl,
                                method = src.DeliveryMethod
                            }
                        };
                        return shipment;
                    });
                });

        }

        private void RegisterOrderStatusDeliveredToB2WDelivered()
        {
            base.CreateMap<OrderStatusDelivered, B2WDeliveryOrder>()
                .ForMember(dest => dest.status, opt => opt.UseValue("complete"))
                .ForMember(dest => dest.delivered_date, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        return DateTime.Now.ToString("dd/MM/yyyy");
                    });
                });

        }

        private void RegisterOrderStatusCancelledToB2WCancelled()
        {
            base.CreateMap<OrderStatusCancel, B2WCancelOrder>()
                .ForMember(dest => dest.status, opt => opt.UseValue("order_canceled"));
        }

        private void RegisterOrderStatusShipExceptionB2WDelivered()
        {
            base.CreateMap<OrderStatusShipException, B2WShipExceptionOrder>()
                .ForMember(dest => dest.OrderId, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var order = (Order)context.Items[MappingContextKeys.Order];

                        return order.IntegrationOrderId;
                    });
                })
                .ForMember(dest => dest.ShipmentException, opt =>
                {
                    opt.ResolveUsing((src, dest, destMember, context) =>
                    {
                        var shipException = new ShipmentException()
                        {
                            OccurrenceDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK"),
                            Observation = src.Reason
                        };

                        return shipException;
                    });
                });

        }
    }
}