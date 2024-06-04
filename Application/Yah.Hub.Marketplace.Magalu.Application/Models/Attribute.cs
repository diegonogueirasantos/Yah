using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class Attribute
    {
        public Attribute() { }
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }


        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
