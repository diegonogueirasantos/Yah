using Yah.Hub.Domain.Order.Interface;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluShipmentLabelRequest
    {
        public string Format { get; set; } = "pdf";

        public string[] OrderIds { get; set; }
    }
}
