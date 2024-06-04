using AutoMapper;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Enums;
using Yah.Hub.Marketplace.Netshoes.Application.Models.Order;

namespace Yah.Hub.Marketplace.Netshoes.Application.Mappings
{
    public class OrderResolver 
        : IValueResolver<Order, Domain.Order.Order, Domain.Order.Address>,
          IValueResolver<Order, Domain.Order.Order, Domain.Order.Customer>,
          IValueResolver<Order, Domain.Order.Order, Domain.Order.Logistic>,
          IValueResolver<Order, Domain.Order.Order, List<Domain.Order.Item>>,
          IValueResolver<Order, Domain.Order.Order, Domain.Order.Payment>,
          IValueResolver<Order, Domain.Order.Order, OrderStatusEnum>
    {
        public Domain.Order.Address Resolve(Order source, Domain.Order.Order destination, Domain.Order.Address destMember, ResolutionContext context)
        {
            var address = source.Shippings.FirstOrDefault()?.Customer.Address;

            return new Domain.Order.Address()
            {
                AddressLine = address.Street,
                Number = address.Number,
                PostalCode = address.PostalCode,
                Neighbourhood = address.Neighborhood,
                City = address.City,
                AddressNotes = address.Complement,
                Landmark = address.Reference
            }.StateNameToUF(address.State);
        }

        public Domain.Order.Customer Resolve(Order source, Domain.Order.Order destination, Domain.Order.Customer destMember, ResolutionContext context)
        {
            var customer = source.Shippings.FirstOrDefault()?.Customer;
            var type = customer.Document.Trim().Length.Equals(11) ? CustomerType.Person : CustomerType.Company;

            return new Domain.Order.Customer()
            {
                Name = customer.CustomerName?.Trim().Split(' ').FirstOrDefault(),
                Surname = string.Join(' ', customer.CustomerName?.Trim().Split(' ').Skip(1)),
                DocumentNumber = customer.Document?.Trim(),
                Type = type,
                Phone = customer.LandLine,
                Cellphone = customer.CellPhone,
                TradingName = type == CustomerType.Company
                              ? (customer.TradeName == "" ? string.Join(' ', customer.CustomerName?.Trim().Split(' ')) : "")
                              : String.Empty
            };
        }

        public Domain.Order.Logistic Resolve(Order source, Domain.Order.Order destination, Domain.Order.Logistic destMember, ResolutionContext context)
        {
            var shipping = source.Shippings.FirstOrDefault();

            return new Domain.Order.Logistic()
            {
                DeliveryLogisticType = shipping.PlatformId.In(new QuotationOrigin[] { QuotationOrigin.NETSHOES_ENTREGAS, QuotationOrigin.MAGALU_ENTREGAS })
                                       ? DeliveryLogisticType.MarketplaceDelivery
                                       : DeliveryLogisticType.SellerFulfillment,
                CarrierName = source.Shippings?.FirstOrDefault()?.Transport?.Carrier,
                ETA  = shipping.DeliveryTime,
                TotalAmount = source.TotalFreight,
                CarrierId = int.Parse(source.Shippings.FirstOrDefault().Transport.CarrierId),
                CarrierReference = source.Shippings.FirstOrDefault().Transport.DeliveryService
            };
        }

        public List<Domain.Order.Item> Resolve(Order source, Domain.Order.Order destination, List<Domain.Order.Item> destMember, ResolutionContext context)
        {
            var shippingItems = source.Shippings.SelectMany(x => x.Items);

            var items = shippingItems.Select(item => new Domain.Order.Item()
            {
                Id = item.SKU,
                Quantity = item.Quantity,
                Price = item.NetUnitValue,
                SalesPrice = item.NetUnitValue,
                Name = item.Name
            }).ToList();

            return items;
        }

        public Domain.Order.Payment Resolve(Order source, Domain.Order.Order destination, Domain.Order.Payment destMember, ResolutionContext context)
        {
            return new Domain.Order.Payment()
            {
                TotalAmount = source.TotalGross,
                Installments = 1,
                InterestAmount = 0,
                DiscountAmount = source.TotalDiscount
            };
        }

        public OrderStatusEnum Resolve(Order source, Domain.Order.Order destination, OrderStatusEnum destMember, ResolutionContext context)
        {
            switch (source.OrderStatus?.ToUpperInvariant())
            {
                case "APPROVED":
                    return OrderStatusEnum.Paid;
                case "INVOICED":
                    return OrderStatusEnum.Invoiced;
                case "SHIPPED": 
                    return OrderStatusEnum.Shipped;
                case "DELIVERED": 
                    return OrderStatusEnum.Delivered;
                case "CANCELED":
                    return OrderStatusEnum.Canceled;
                case "WAITING CHECK-IN": 
                    return OrderStatusEnum.WaitingExchange;
                case "FROZEN": 
                    return OrderStatusEnum.WaitingReview;
                case "CREATED":
                default:
                    return OrderStatusEnum.WaitingPayment;
            }
        }
    }
}
