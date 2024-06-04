using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Order
{
    public class Address
    {
        [JsonProperty("number")] 
        public string Number { get; set; }

        [JsonProperty("complement")] 
        public string Complement { get; set; }

        [JsonProperty("neighborhood")] 
        public string Neighborhood { get; set; }

        [JsonProperty("reference")] 
        public string Reference { get; set; }

        [JsonProperty("city")] 
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("street")] 
        public string Street { get; set; }

        [JsonProperty("postalCode")] 
        public string PostalCode { get; set; }
    }
}
