using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class Dimension
    {
        /// <summary>
        /// A unidade de medida é METROS. O valor mínimo deve ser: 0.01 e o máximo: 7. O separador decimal é PONTO.
        /// </summary>
        [JsonProperty("altura")]
        public decimal Height { get; set; }

        /// <summary>
        /// A unidade de medida é METROS. O valor mínimo deve ser: 0.01 e o máximo: 7. O separador decimal é PONTO.
        /// </summary>
        [JsonProperty("largura")]
        public decimal Width { get; set; }

        /// <summary>
        /// A unidade de medida é METROS. O valor mínimo deve ser: 0.01 e o máximo: 7. O separador decimal é PONTO.
        /// </summary>
        [JsonProperty("profundidade")]
        public decimal Depth { get; set; }

        /// <summary>
        /// A unidade de medida é QUILOGRAMA (kg) e valor mínimo deve ser: 0.01 e o máximo: 750. O separador decimal é PONTO.
        /// </summary>
        [JsonProperty("peso")]
        public decimal Weight { get; set; }
    }
}
