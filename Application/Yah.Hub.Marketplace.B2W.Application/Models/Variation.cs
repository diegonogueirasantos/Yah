using Newtonsoft.Json;
using Yah.Hub.Marketplace.B2W.Application.Models;

namespace Yah.Hub.Marketplace.B2W.Application.Models
{
    public class VariationWrapper
    {
        [JsonProperty("variation")]
        public Variation Variation { get; set; }
    }

    public class Variation
    {
        #region Basic Info
        [JsonProperty("sku", Required = Required.Always)]
        public string Sku { get; set; }

        [JsonProperty("qty", NullValueHandling = NullValueHandling.Ignore)]
        public int? Inventory { get; set; } = null;
        #endregion

        #region Additional Info
        [JsonProperty("ean", NullValueHandling = NullValueHandling.Ignore)]
        public string EAN { get; set; }
        #endregion

        #region Images
        [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ImagesUrls { get; set; }
        #endregion

        #region Specifications
        [JsonProperty("specifications", NullValueHandling = NullValueHandling.Ignore)]
        public List<Specification> Specifications { get; set; }
        #endregion
    }
}
