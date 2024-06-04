using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class Price
    {
        /// <summary>
        /// Preço POR do produto. Valor mínimo: 0,01 e valor máximo: 99999,00.
        /// </summary>
        [JsonProperty("oferta")]
        public string SalePrice { get; set; }

        /// <summary>
        /// Preço DE do produto. Valor mínimo: 0.01 e valor máximo: 99999.00.
        /// </summary>
        [JsonProperty("padrao")]
        public string ListPrice { get; set; }
    }
}
