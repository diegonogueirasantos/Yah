using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Domain.Order;
using Yah.Hub.Domain.OrderStatus;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Mappings
{
    public class SaleMappingProfile : Profile
    {
        public SaleMappingProfile()
        {
            this.RegisterMeliOrderToOrder();
            this.RegisterOrderStatusInvoiceToMeliInvoice();
        }


        private void RegisterMeliOrderToOrder()
        {
            base.CreateMap<MeliOrder, Order>()
                .ForMember(dest => dest.IntegrationOrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Marketplace, opt => opt.UseValue("mercadolivre"))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.Items, opt =>
                {
                    #region [Code]
                    opt.ResolveUsing((src) =>
                    {
                        return src.OrderItems.Select(x =>
                        {
                            return new Domain.Order.Item()
                            {
                                Id = x.Item.SellerSku,
                                ExternalId = x.Item.SellerCustomField,
                                Quantity = x.Quantity ?? 0,
                                Price = x.FullUnitPrice ?? 0,
                                SalesPrice = x.UnitPrice ?? 0,
                                Name = x.Item.Title,
                                ReferenceId = x.Item.VariationId
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
                        var type = src.Buyer?.BillingInfo?.DocType == "CNPJ" ? Common.Enums.CustomerType.Company : Common.Enums.CustomerType.Person;
                        var phone = $"{src.Buyer?.Phone?.AreaCode ?? "00"}{src.Buyer?.Phone?.Number ?? "000000000"}".Trim().OnlyNumbers().Truncate(12);

                        return new Domain.Order.Customer()
                        {
                            Type = type,
                            Name = src.Buyer?.FirstName,
                            Surname = src.Buyer?.LastName,
                            Email = src.Buyer?.Email ?? $"{src.Buyer?.FirstName}.{src.Buyer?.LastName}.{src.Buyer?.BillingInfo?.DocNumber}@Yahhub{MarketplaceAlias.MercadoLivre}.com.br",
                            Gender = Common.Enums.Gender.Male, //Mercado Livre não informa o gênero
                            DocumentNumber = src.Buyer?.BillingInfo?.DocNumber?.Trim() ?? String.Empty,
                            Phone = phone,
                            Cellphone = phone,//Mercado Livre não da detalhe do tipo de phone
                            TradingName = type == Common.Enums.CustomerType.Company ? $"{src.Buyer?.FirstName} {src.Buyer?.LastName}" : String.Empty,
                            StateInscription = src.Buyer?.BillingInfo?.AdditionalInfo?.Where(x => x.Type == "STATE_REGISTRATION").FirstOrDefault()?.Value ?? String.Empty
                        };
                    });
                    #endregion
                })
                .ForMember(dest => dest.ShippingAddress, opt =>
                {
                    #region [Code]
                    opt.ResolveUsing((src) =>
                    {
                        if(src.Shipping == null)
                        {
                            return null;
                        }

                        var neighbourhood = src.Shipping.Destination?.ShippingAddress?.Neighborhood?.Name;
                        if (string.IsNullOrWhiteSpace(neighbourhood))
                            neighbourhood = "Bairro não informado";
                        
                        return new Domain.Order.Address()
                        {
                            AddressLine = src.Shipping?.Destination?.ShippingAddress?.StreetName,
                            Number = src.Shipping?.Destination?.ShippingAddress?.StreetNumber,
                            PostalCode = src.Shipping?.Destination?.ShippingAddress?.ZipCode,
                            Neighbourhood = neighbourhood,
                            City = src.Shipping?.Destination?.ShippingAddress?.City?.Name,
                            State = src.Shipping?.Destination?.ShippingAddress?.State?.Id?.Split('-').Last(),
                            AddressNotes = src.Shipping?.Destination?.ShippingAddress?.Comment,
                            FullName = src.Shipping?.Destination?.ReceiverName
                        };
                    });
                    #endregion
                })
                .ForMember(dest => dest.BillingAddress, opt =>
                {
                    #region[Code]
                    opt.PreCondition(src => src.Buyer?.BillingInfo != null);
                    opt.ResolveUsing((src) =>
                    {
                        var neighbourhood = src.Shipping.Destination?.ShippingAddress?.Neighborhood?.Name;
                        if (string.IsNullOrWhiteSpace(neighbourhood))
                            neighbourhood = "Bairro não informado";

                        var addressInfo = src.Buyer.BillingInfo?.AdditionalInfo?.ToDictionary(x => x.Type, x => x.Value);
                        
                        var fullName = string.Concat(addressInfo.GetValueOrDefault("FIRST_NAME"), " ", addressInfo.GetValueOrDefault("LAST_NAME"));
                        if (string.IsNullOrWhiteSpace(fullName))
                            fullName = (string.Concat(src.Buyer.FirstName, " ", src.Buyer.LastName));

                        var state = src.Shipping.Destination?.ShippingAddress?.State?.Id.Split('-').Last();


                        return new Domain.Order.Address()
                        {
                            AddressLine = addressInfo.GetValueOrDefault("STREET_NAME"),
                            Number = addressInfo.GetValueOrDefault("STREET_NUMBER"),
                            PostalCode = addressInfo.GetValueOrDefault("ZIP_CODE"),
                            Neighbourhood = neighbourhood,
                            City = addressInfo.GetValueOrDefault("CITY_NAME"),
                            AddressNotes =addressInfo.GetValueOrDefault("COMMENT"),
                            FullName = fullName
                        }
                        .StateNameToUF(addressInfo.GetValueOrDefault("STATE_NAME"));
                    });
                    #endregion
                })
                .ForMember(dest => dest.Logistic, opt =>
                {
                    #region [Code]
                    opt.ResolveUsing((src) =>
                    {
                        var shippingName = (src.Shipping.LeadTime?.ShippingMethod?.Name ??
                                        src.Shipping.ShippingOption?.Name ??
                                        string.Empty)
                                        .ToLowerInvariant();
                        
                        var EstimatedDate = src.Shipping.LeadTime?.EstimatedDeliveryTime?.Date ?? src.Shipping.ShippingOption?.EstimatedDeliveryTime?.Date;
                        var eta = (short)Math.Max(0, (EstimatedDate - src.DateCreated)?.Days ?? 0);

                        var logisticType = DeliveryLogisticType.SellerFulfillment;

                        if (src.Shipping.Logistic != null)
                            logisticType = src.Shipping.Logistic.Type == "fulfillment" ? DeliveryLogisticType.MarketplaceFulfillment : DeliveryLogisticType.MarketplaceDelivery;

                        return new Domain.Order.Logistic() {
                            TotalAmount = src.Shipping.LeadTime?.Cost ?? src.Shipping.ShippingOption?.Cost ?? 0m,
                            ETA = src.DateCreated.WorkingDays(eta),
                            DeliveryLogisticType = logisticType,
                            TrackingCode = src.Shipping.TrackingNumber,
                            CarrierName = this.SetCarrierName(shippingName),
                            CarrierId = src.Shipping.ServiceId ?? 0
                        };
                    });
                    #endregion
                })
                .ForMember(dest => dest.OrderStatus, opt =>
                {
                    #region [Code]

                    opt.ResolveUsing((src) =>
                    {
                        var orderStatus = src.Status;
                        var shippingStatus = src.Shipping.Status;
                        var tags = src.Tags;

                        if (orderStatus.Equals("cancelled") || (orderStatus.Equals("paid") && tags.Contains("unfulfilled")))
                            return OrderStatusEnum.Canceled;

                        if(src.Payments.All(x => "refunded".Equals(x.Status, StringComparison.InvariantCultureIgnoreCase)))
                            return OrderStatusEnum.Refunded;

                        if (shippingStatus != null)
                        {
                            if (shippingStatus.Equals("delivered") || tags.Contains("delivered"))
                                return OrderStatusEnum.Delivered;
                            
                            if (shippingStatus.Equals("shipped") || tags.Contains("delivered"))
                                return OrderStatusEnum.Shipped;
                        }

                        if (orderStatus.Equals("approved") || orderStatus.Equals("paid") || tags.Contains("paid"))
                            return OrderStatusEnum.Paid;
                        
                        return OrderStatusEnum.WaitingPayment;
                    });

                    #endregion
                });
        }

        private void RegisterOrderStatusInvoiceToMeliInvoice()
        {
            base.CreateMap<OrderStatusInvoice, MeliInvoice>()
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.InvoiceData, opt => 
                {
                    opt.ResolveUsing((src) =>
                    {
                        return new InvoiceData()
                        {
                            CFOP = src.CFOP,
                            IE = src.IE
                        };
                    });
                });
                
        }

        #region [Private Methods]
        private string SetCarrierName(string shippingName)
        {
            #region [Code]
            String carrierName;

            switch (shippingName)
            {
                case "normal":
                    carrierName = "me_classico";
                    break;
                case "expresso":
                    carrierName = "me_expresso";
                    break;
                case "prioritário":
                    carrierName = "me_prioritario";
                    break;
                default:
                    carrierName = "N/A";
                    break;
            }

            return carrierName;
            #endregion
        }
        #endregion
    }
}
