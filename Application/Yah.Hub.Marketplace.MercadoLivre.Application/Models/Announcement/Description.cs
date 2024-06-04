using Newtonsoft.Json;
namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class Description
    {
        [JsonProperty("plain_text")]
        public string PlainText { get; set; }
    }
}
