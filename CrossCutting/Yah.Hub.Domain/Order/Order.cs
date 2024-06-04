using Yah.Hub.Common.AbstractRepositories.ElasticSearch;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.Security;
using Yah.Hub.Domain.Order.Interface;
using System.Runtime.CompilerServices;

namespace Yah.Hub.Domain.Order
{
    public class Order : BaseEntity, IOrderReference
    {
        #region [Constructor]
        public Order(string id)
        {
            this.Id = id;
        }
        #endregion

        #region [Properties]
        public string TenantId { get; set; }
        
        public string VendorId { get; set; }

        public string AccountId { get; set; }

        public Guid OrderId { get; set; }

        public OrderStatusEnum OrderStatus { get; set; }

        public string ExternalOrderId { get; set; }

        public string IntegrationOrderId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public string Marketplace { get; set; }

        public List<Item> Items { get; set; }

        public Customer Customer { get; set; }

        public Address ShippingAddress { get; set; }

        public Address BillingAddress { get; set; }

        public Logistic Logistic { get; set; }

        public Invoice Invoice { get; set; }

        public Payment Payment { get; set; }

        public string MarketplaceOrderQueueId { get; set; }

        public string MarketplaceBrand { get; set; }
        #endregion

        #region [Methods]

        public void SetIdentity(Identity identity)
        {
            this.TenantId = identity.GetTenantId();
            this.VendorId = identity.GetVendorId();
            this.AccountId = identity.GetAccountId();
        }

        #endregion
    }


}
