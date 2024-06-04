using System;
using Newtonsoft.Json;
using Yah.Hub.Domain.OrderStatus;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluInvoiceOrder : MagaluOrderStatus
    {
        [JsonProperty("InvoicedNumber")]
        public string InvoicedNumber { get; set; }

        [JsonProperty("InvoicedLine")]
        public string InvoicedLine { get; set; }

        [JsonProperty("InvoicedIssueDate")]
        public DateTimeOffset? InvoicedIssueDate { get; set; }

        [JsonProperty("InvoicedKey")]
        public string InvoicedKey { get; set; }

        [JsonProperty("InvoicedDanfeXml")]
        public string InvoicedDanfeXml { get; set; }
    }
}