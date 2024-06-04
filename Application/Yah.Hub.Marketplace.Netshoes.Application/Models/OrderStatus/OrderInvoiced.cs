using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.OrderStatus
{
    public class OrderInvoiced
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("line")]
        public string Line { get; set; }

        [JsonProperty("issueDate")]
        public DateTime? IssueDate { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("danfeXml")]
        public string? DanfeXml { get; set; }
    }
}
