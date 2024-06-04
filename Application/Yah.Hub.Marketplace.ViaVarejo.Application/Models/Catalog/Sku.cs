using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class Sku
    {
        /// <summary>
        /// Deve-se informar o código SKU do lojista.
        /// Máximo de 30 caracteres e só é permitido letras, números, (-) hífen e/ou (_)underline. Outros caracteres como: ponto, asterisco, dentre outros, não são permitidos.
        /// </summary>
        [JsonProperty("idSkuLojista")]
        public string SellerSkuId { get; set; }

        /// <summary>
        /// Obrigatório respeitar o registro da GS1Brasil. O EAN deve conter:8,9,12,13 ou 14 caracteres. 
        /// Recomendamos o envio do código para haver sugestão de match.
        /// </summary>
        [JsonProperty("gtin")]
        public string Gtin { get; set; }

        /// <summary>
        /// O campo aceita no mínimo 1 e no máximo 4 imagens por SKU
        /// </summary>
        [JsonProperty("imagens")]
        public List<string> Imagens { get; set; }

        [JsonProperty("preco")]
        public Price Price { get; set; }

        [JsonProperty("estoque")]
        public Inventory Inventory { get; set; }

        [JsonProperty("dimensao")]
        public Dimension Dimension { get; set; }

        [JsonProperty("atributos")]
        public List<Attribute> Attributes { get; set; }

        [JsonProperty("skuStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ViaVarejoProductStatus? Status { get; set; }

        [JsonProperty("percentualAssociacao", NullValueHandling = NullValueHandling.Ignore)]
        public MatchAssociation MatchAssociation { get; set; }

        [JsonProperty("violacoes", NullValueHandling = NullValueHandling.Ignore)]
        public Violation[] Violation { get; set; }
    }
}
