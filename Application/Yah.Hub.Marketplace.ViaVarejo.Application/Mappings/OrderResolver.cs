using AutoMapper;
using Nest;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Marketplace.ViaVarejo.Application.Models.Order;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Mappings
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
            var address = source.ShippingAddress;

            return new Domain.Order.Address()
            {
                AddressLine = address.Street,
                Number = address.Number,
                PostalCode = address.ZipCode,
                Neighbourhood = address.Quarter,
                City = address.City,
                AddressNotes = address.Complement,
                Landmark = address.Reference
            }.StateNameToUF(address.State);
        }

        public Domain.Order.Customer Resolve(Order source, Domain.Order.Order destination, Domain.Order.Customer destMember, ResolutionContext context)
        {
            var customer = source.Customer;
            var type = customer.DocumentNumber.Trim().Length.Equals(11) ? CustomerType.Person : CustomerType.Company;

            var phone = source.Customer.Phones?
                .Where(x => "HOME".Equals(x.Type, StringComparison.InvariantCultureIgnoreCase))?
                .Select(x => x.Number).FirstOrDefault();

            var cellphone = source.Customer.Phones?
                .Where(x => "MOBILE".Equals(x.Type, StringComparison.InvariantCultureIgnoreCase))?
                .Select(x => x.Number).FirstOrDefault();

            return new Domain.Order.Customer()
            {
                Name = customer.Name,
                Surname = string.Empty,
                DocumentNumber = customer.DocumentNumber,
                Type = type,
                Phone = phone,
                Cellphone = cellphone,
                TradingName = type == CustomerType.Company ? customer.Name : null
            };
        }

        public Domain.Order.Logistic Resolve(Order source, Domain.Order.Order destination, Domain.Order.Logistic destMember, ResolutionContext context)
        {
            var logisticType = source.ItemsSummary.Any(x => x.FreightItemsSummary.IsEnvvias)
                               ? DeliveryLogisticType.MarketplaceDelivery
                               : DeliveryLogisticType.SellerFulfillment;

            var carrier = source.Trackings.FirstOrDefault().Carrier;

            var eta = DateTimeOffset.Parse(source.PurchasedAt).WorkingDays(Convert.ToInt16(DateTime.Parse(source.PromisedDeliveryDate).Subtract(DateTime.Parse(source.PurchasedAt)).TotalDays));

            return new Domain.Order.Logistic()
            {
                DeliveryLogisticType = logisticType,
                CarrierName = carrier.Name,
                ETA = eta,
                TotalAmount = decimal.Parse(source.Freight.ChargedAmount),
                CarrierReference = carrier.Cnpj 
            };
        }

        public List<Domain.Order.Item> Resolve(Order source, Domain.Order.Order destination, List<Domain.Order.Item> destMember, ResolutionContext context)
        {
            var items = source.Items.GroupBy(x => x.SkuSellerId).Select(i => new Domain.Order.Item()
            {
                Id = i.First().SkuSellerId,
                Quantity = i.Count(),
                Price = i.First().SalePrice,
                SalesPrice = i.First().SalePrice,
                Name = i.First().Name,
            }).ToList();

            return items;
        }

        public Domain.Order.Payment Resolve(Order source, Domain.Order.Order destination, Domain.Order.Payment destMember, ResolutionContext context)
        {
            var installments = source.Payment.Where(x => x.PaymentType == 1).FirstOrDefault()?.Installments ?? 1;

            return new Domain.Order.Payment()
            {
                TotalAmount = Math.Round(source.TotalAmount, 2),
                Installments = installments,
                InterestAmount = 0,
                DiscountAmount = 0
            };
        }

        public OrderStatusEnum Resolve(Order source, Domain.Order.Order destination, OrderStatusEnum destMember, ResolutionContext context)
        {
            switch (source.Status)
            {
                case OrderStatusAlias.CAN: // Cancelado
                case OrderStatusAlias.DVC: // Devolução concluída
                    return OrderStatusEnum.Canceled;
                case OrderStatusAlias.PAY: // Pagamento aprovado
                    return OrderStatusEnum.Paid;
                case OrderStatusAlias.PDL: // Parcialmente entregue, considerar como enviado
                case OrderStatusAlias.SHP: // Enviado
                    return OrderStatusEnum.Shipped;
                case OrderStatusAlias.DLV: // Entregue
                    return OrderStatusEnum.Delivered;
                case OrderStatusAlias.PEN: // Pagamento pendente
                    return OrderStatusEnum.WaitingPayment;
                default: // "NEW"
                    return OrderStatusEnum.WaitingPayment;
            }
        }
    }
}
