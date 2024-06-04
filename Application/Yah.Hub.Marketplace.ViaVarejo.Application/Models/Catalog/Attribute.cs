using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class Attribute
    {
        /// <summary>
        /// Código do atributo
        /// </summary>
        [JsonProperty("idUda")]
        public string Id { get; set; }

        /// <summary>
        /// Valor da atributo
        /// </summary>
        [JsonProperty("valor")]
        public string Value { get; set; }
    }
}
