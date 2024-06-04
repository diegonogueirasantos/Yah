using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class MatchAssociation
    {
        /// <summary>
        /// Lista de opções de Match de produtos da Via Varejo
        /// </summary>
        [JsonProperty("listaSkuMatchInteligenteResultado", NullValueHandling = NullValueHandling.Ignore)]
        public List<MatchSKUOption> MatchOptions { get; set; }
    }

    public class MatchSKUOption
    {
        /// <summary>
        /// Código do SKU proposto pela Via Varejo para Match.
        /// </summary>
        [JsonProperty("idSkuPossivel")]
        public string Sku { get; set; }

        /// <summary>
        /// Percentual de similaridade do produto do seller com as opções da Via Varejo.
        /// </summary>
        [JsonProperty("percentual")]
        public int Percentage { get; set; }
    }
}
