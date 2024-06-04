using System;
using System.Text.Json.Serialization;

namespace Yah.Hub.Common.ServiceMessage
{
    public class Error
    {
        /// <summary>
        /// What happened?
        /// EX: Unable to create product
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Why it happened?
        /// EX: Marketplace server responses 503 due a temporary service unavailable
        /// </summary>
        public string Reason { get; set; }
        public ErrorType Type { get; set; }
        [JsonIgnore]
        public Exception? OriginalExcetion { get; private set; }

        public Error(string message, string reason, ErrorType type)
        {
            this.Message = message;
            this.Reason = reason;
            this.Type = type;
        }

        public Error(Exception ex)
        {
            this.Message = ex?.Message ?? "Exception is Null";
            this.Reason = ex?.StackTrace ?? "Empty StackTrace";
            this.Type = ErrorType.Technical;
            this.OriginalExcetion = ex;
        }

        public Error(Exception ex, string errorMsg)
        {
            this.Message = errorMsg;
            this.Reason = ex.StackTrace ?? "Empty StackTrace";
            this.Type = ErrorType.Technical;
            this.OriginalExcetion = ex;
        }
    }
}

