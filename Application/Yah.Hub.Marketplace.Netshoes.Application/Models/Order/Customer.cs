using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Customer
    {
        [JsonProperty("document")] 
        public string Document { get; set; }

        [JsonProperty("stateInscription")] 
        public string StateInscription { get; set; }

        [JsonProperty("tradeName")] 
        public string TradeName { get; set; }

        [JsonProperty("customerName")] 
        public string CustomerName { get; set; }

        [JsonProperty("recipientName")] 
        public string RecipientName { get; set; }

        [JsonProperty("cellPhone")] 
        public string CellPhone { get; set; }

        [JsonProperty("landLine")] 
        public string LandLine { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }
    }
}
