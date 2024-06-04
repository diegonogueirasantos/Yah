using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models.Order
{
    public class B2WInvoice
    {
        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("volume_qty")]
        public int volume_qty { get; set; }

        [JsonProperty("issue_date")]
        public DateTime issue_date { get; set; }
    }

    public class B2WInvoiceOrder : B2WOrderStatus
    {
        [JsonProperty("invoice")]
        public B2WInvoice invoice { get; set; }
    }
}