using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class MagaluError
    {
        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("Errors")]
        public List<MagaluErrorItem> Errors { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }
    }

    public class MagaluErrorItem
    {
        [JsonProperty("Field")]
        public string Field { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}
