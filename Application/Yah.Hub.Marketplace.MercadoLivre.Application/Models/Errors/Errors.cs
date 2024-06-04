using Newtonsoft.Json;
using System.Net;
using Yah.Hub.Common.Extensions;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Errors
{
    public class Errors
    {
    }

    public class ErrorResult
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string ErrorCode { get; set; }

        [JsonProperty("status")]
        public HttpStatusCode HttpStatus { get; set; }

        [JsonProperty("cause")]
        [JsonConverter(typeof(ListOrArrayConverter<Reason>))]
        public List<Reason> Reasons { get; set; }
    }

    public class Reason
    {
        [JsonProperty("department")]
        public string department { get; set; }

        [JsonProperty("cause_id")]
        public int cause_id { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("code")]
        public string ReasonCode { get; set; }

        [JsonProperty("references")]
        public List<string> references { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

    }
}
