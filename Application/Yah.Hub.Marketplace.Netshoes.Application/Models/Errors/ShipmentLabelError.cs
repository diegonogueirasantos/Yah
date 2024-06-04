using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Netshoes.Application.Models.Errors
{
    public class ShipmentLabelError
    {
        [JsonProperty("errors")]
        public List<Error> Errors { get; set; }
    }

    public class Error
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("informationCodes")]
        public List<int> InformationCodes { get; set; }
    }
}
