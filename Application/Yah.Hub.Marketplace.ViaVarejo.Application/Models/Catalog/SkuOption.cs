using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class SkuOption
    {
        public SkuOption(string optionType, string selectedSku, string productId)
        {
            OptionType = optionType;
            SelectedSku = selectedSku;
            ProductId = productId;
        }


        /// <summary>
        /// Neste campo, deve-se indicar como deseja que o item seja indexado.
        /// NEW_SKU = O produto será indexado como item novo (em uma nova página no front) e sem relevância.
        /// MATCH_SKU= O produto será indexado já associado ao produto sugerido e disputará buybox na mesma página (ganha a relevância do detentor do anúncio).
        /// </summary>
        [JsonProperty("resultado")]
        public string OptionType { get; set; }

        /// <summary>
        /// Informar o código do SKU sugerido, que é disponibilizado no campo idSkuPossivel.
        /// Este campo só  é preenchido caso a opção escolhida em resultado seja MATCH_SKU.
        /// </summary>
        [JsonProperty("idSkuEscolhido", NullValueHandling = NullValueHandling.Include)]
        public string SelectedSku { get; set; }
        
        [JsonIgnore]
        public string ProductId { get; set; }
    }
}
