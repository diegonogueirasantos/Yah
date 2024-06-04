using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class Inventory
    {
        /// <summary>
        /// Aceitamos o valor= 0. Caso o campo não seja passado ou esteja em branco, automaticamente converteremos para 0.
        /// </summary>
        [JsonProperty("quantidade")]
        public int Quantity { get; set; }

        /// <summary>
        /// Refere-se ao crossdocking do produto. O valor indicado neste campo, 
        /// será utilizado na composição do frete nos casos de tabela de contingência e/ou Envvias. 
        /// Pode-se passar o valor= 0 e no máximo = 200
        /// </summary>
        [JsonProperty("tempoDePreparacao")]
        public int HandlingTime { get; set; }
    }
}
