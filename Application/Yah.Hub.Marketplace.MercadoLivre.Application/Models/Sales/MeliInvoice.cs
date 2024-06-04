using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Sales
{
    public class MeliInvoice
    {
        [JsonProperty("fiscal_key")]
        public string Key { get; set; }

        [JsonProperty("additional_data")]
        public InvoiceData InvoiceData { get; set; }
    }

    public class InvoiceData
    {
        [JsonProperty("cfop")]
        public string CFOP { get; set; }

        [JsonProperty("company_state_tax_id")]
        public string IE { get; set; }
    }
}
