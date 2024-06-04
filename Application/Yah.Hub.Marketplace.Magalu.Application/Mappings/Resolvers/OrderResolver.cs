using AutoMapper;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Domain.Order;
using Yah.Hub.Marketplace.Magalu.Application.Models;

namespace Yah.Hub.Marketplace.Magalu.Application.Mappings.Resolvers
{
    public class OrderResolver :
        IValueResolver<MagaluOrder, Order, Customer>,
        IValueResolver<MagaluOrder, Order, Address>,
        IValueResolver<MagaluOrder, Order, OrderStatusEnum>,
        IValueResolver<MagaluOrder, Order, Payment>,
        IValueResolver<MagaluOrder, Order, Logistic>,
        IValueResolver<MagaluOrder, Order, List<Item>>
    {
        public Customer Resolve(MagaluOrder source, Order destination, Customer destMember, ResolutionContext context)
        {
            #region [Code]
            var document = !String.IsNullOrWhiteSpace(source.CustomerPjCnpj) ? source.CustomerPjCnpj : source.CustomerPfCpf;
            var type = !String.IsNullOrEmpty(source.CustomerPjCnpj) ? CustomerType.Company : CustomerType.Person;

            string name = string.Empty;
            string surname = string.Empty;

            if (type.Equals(CustomerType.Person))
            {
                name = source.CustomerPfName.Split(' ').First();
                surname = String.Join(' ', source.CustomerPfName.Split(' ').Skip(name.Length));
            }
            else
            {
                name = source.CustomerPjCorporatename;
            }

            return new Customer()
            {
                Type = type,
                DocumentNumber = document,
                Email = !string.IsNullOrWhiteSpace(source.CustomerMail) ? source.CustomerMail : $"hub-{document}@Yah.hubmarketplace.com.br",
                Name = name,
                Surname = surname,
                Gender = Gender.Male,
                Cellphone = source.TelephoneMainNumber,
                Phone = !string.IsNullOrWhiteSpace(source.TelephoneSecundaryNumber) ? source.TelephoneSecundaryNumber : source.TelephoneMainNumber,
                StateInscription = source.CustomerPjCnpj
            };
            #endregion
        }

        public Address Resolve(MagaluOrder source, Order destination, Address destMember, ResolutionContext context)
        {
            #region [Code]
            return new Address()
            {
                AddressLine = source.DeliveryAddressStreet,
                Number = source.DeliveryAddressNumber,
                Neighbourhood = source.DeliveryAddressNeighborhood,
                Landmark = source.DeliveryAddressReference,
                AddressNotes = source.DeliveryAddressAdditionalInfo,
                City = source.DeliveryAddressCity,
                State = source.DeliveryAddressState,
                PostalCode = source.DeliveryAddressZipcode,
                FullName = !string.IsNullOrEmpty(source.CustomerPfName) ? source.CustomerPfName : source.CustomerPjCorporatename
            };
            #endregion
        }

        public OrderStatusEnum Resolve(MagaluOrder source, Order destination, OrderStatusEnum destMember, ResolutionContext context)
        {
            #region [Code]
            switch (source.OrderStatus)
            {
                case "CANCELED":
                case "UNAVAILABLE":
                    return OrderStatusEnum.Canceled;

                case "SHIPMENT_EXCEPTION":
                    return OrderStatusEnum.ShipmentException;

                case "DELIVERED":
                    return OrderStatusEnum.Delivered;

                case "SHIPPED":
                    return OrderStatusEnum.Shipped;

                case "INVOICED":
                    return OrderStatusEnum.Invoiced;

                case "PROCESSING":
                case "APPROVED":
                    return OrderStatusEnum.Paid;

                default:
                    return OrderStatusEnum.WaitingPayment;
            }
            #endregion
        }

        public Payment Resolve(MagaluOrder source, Order destination, Payment destMember, ResolutionContext context)
        {
            #region [Code]
            return new Payment()
            {
                TotalAmount = source.TotalAmount ?? 0m,
                DiscountAmount = source.TotalDiscount ?? 0m,
                InterestAmount = source.TotalTax ?? 0m,
                Installments = source.Payments.Max(x => x.Installments) ?? 0,
            };
            #endregion
        }

        public Logistic Resolve(MagaluOrder source, Order destination, Logistic destMember, ResolutionContext context)
        {
            #region [Code]
            var magaluDeliveryTypes = new List<string>() { "magalu entregas", "magalog" };

            return new Logistic()
            {
                DeliveryLogisticType = magaluDeliveryTypes.Contains(source.ShippedCarrierName) ? DeliveryLogisticType.MarketplaceDelivery : DeliveryLogisticType.SellerFulfillment,
                CarrierName = source.ShippedCarrierName ?? string.Empty,
                TotalAmount = source.TotalFreight ?? 0m,
                ETA = source.PurchasedDate.Value.WorkingDays((short)Math.Max(0, (source.DeliveredDate - source.PurchasedDate)?.Days ?? 0))
            };
            #endregion
        }

        public List<Item> Resolve(MagaluOrder source, Order destination, List<Item> destMember, ResolutionContext context) =>
       
            #region [Code]
            source.Products.Select(x => new Item()
            {
                ExternalId = x.IdSku,
                Quantity = x.Quantity ?? 0,
                Price = x.Price ?? 0,
                SalesPrice =  x.Price ?? 0,
                Name = $"Product {x.IdSku}"
            }).ToList();
        #endregion
    }
}
