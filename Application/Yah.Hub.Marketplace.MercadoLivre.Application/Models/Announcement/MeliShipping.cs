using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class MeliShipping
    {
        [JsonProperty("mode")]
        public string ShippingMode { get; set; }

        [JsonProperty("free_methods")]
        public FreeMethod FreeMethods { get; set; }

        [JsonProperty("free_shipping")]
        public bool FreeShipping { get; set; }

        [JsonProperty("logistic_type")]
        public string LogisticType { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("dimensions")]
        [JsonConverter(typeof(DimensionsConverter))]
        public Dimensions Dimensions { get; set; }
    }

    public class FreeMethod
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        public Rule Rule { get; set; } = new Rule();
    }

    public class Rule
    {

        [JsonProperty("free_mode")]
        public string FreeMode { get; set; }

        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }
}
