using Newtonsoft.Json;

namespace Yah.Hub.Common.ServiceMessage
{
    public class ServiceMessageResult
    {
        [JsonProperty("errors")]
        public virtual List<ErrorResult>? Errors { get; set; } = new List<ErrorResult>();
    }

    public class ServiceMessageResult<T> : ServiceMessageResult
    {
        [JsonProperty("errors")]
        public virtual List<ErrorResult>? Errors { get; set; } = new List<ErrorResult>();

        [JsonProperty("data")]
        public T Data { get; set; }
    }

    public class ErrorResult
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("type")]
        public ErrorType Type { get; set; }
    }
}
