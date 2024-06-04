using System;
using Yah.Hub.Common.Extensions;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Category;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Infractions;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Token;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Client
{
    public interface IMercadoLivreClient
    {
        public Task<HttpMarketplaceMessage<Token>> GetAccessToken(MarketplaceServiceMessage<string> code);
        public Task<HttpMarketplaceMessage<Token>> RefreshToken(MarketplaceServiceMessage previousToken);
        public Task<HttpMarketplaceMessage<byte[]>> GetShipmentLabel(MarketplaceServiceMessage<string> shippingOrderId);
        public Task<MarketplaceServiceMessage<string>> GetAuthorizationUrl(MarketplaceServiceMessage message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> GetMeliAnnouncement(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> CreateAnnoucement(MarketplaceServiceMessage<MeliAnnouncement> message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> UpdateAnnoucement(MarketplaceServiceMessage<MeliAnnouncement> message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> UpdateAnnoucementInventory(MarketplaceServiceMessage<MeliAnnoucementInventory> message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> UpdateAnnoucementPrice(MarketplaceServiceMessage<MeliAnnoucementPrice> message);

        public Task<HttpMarketplaceMessage<List<MeliCategory>>> GetCategory(MarketplaceServiceMessage<string?> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage<List<MeliCategoryAttribute>>> GetCategoryAttributes(MarketplaceServiceMessage<string> marketplaceServiceMessage);

        public Task<HttpMarketplaceMessage> CreateAnnoucementDescription(MarketplaceServiceMessage<MeliAnnouncement> message);
        public Task<HttpMarketplaceMessage> UpdateAnnoucementDescription(MarketplaceServiceMessage<MeliAnnouncement> message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> SetAnnoucementStatus(MarketplaceServiceMessage<UpdateAnnoucementStatus> message);
        public Task<HttpMarketplaceMessage<MeliAnnouncement>> DeleteAnnoucement(MarketplaceServiceMessage<UpdateAnnoucementStatus> message);
        public Task<HttpMarketplaceMessage<OrderClientResult>> GetOrdersForIntegration(MarketplaceServiceMessage<OrderQueryRequest> message);
        public Task<HttpMarketplaceMessage<MeliOrder>> GetOrder(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage<Models.Sales.Shipping>> GetMeliShipping(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage<BillingInfoRequest>> GetBillingInfo(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> DeliveryOrder(MarketplaceServiceMessage<string> message);
        public Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<(string shippingId, string invoiceXML)> message);
        public Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<(string shippingId, MeliInvoice invoice)> message);
        public Task<HttpMarketplaceMessage> InvoiceOrder(MarketplaceServiceMessage<(string packId, byte[] xml, string fileName)> message);
        public Task<HttpMarketplaceMessage> ShippedOrder(MarketplaceServiceMessage<(string shippingId, string trackingNumber, int serviceId, long? buyer)> message);
        public Task<HttpMarketplaceMessage<MercadoLivreInfractions>> GetMeliInfractionsByAnnouncementId(MarketplaceServiceMessage<string> marketplaceServiceMessage);
    }
}

