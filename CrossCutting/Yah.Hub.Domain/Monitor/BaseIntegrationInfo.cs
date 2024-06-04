using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Yah.Hub.Common.Enums;
using Yah.Hub.Common.Identity;
using Yah.Hub.Common.Marketplace;
using Yah.Hub.Common.ServiceMessage;
using Yah.Hub.Domain.Catalog;

namespace Yah.Hub.Domain.Monitor
{
    public class BaseIntegrationInfo 
    {
        #region Properties

        public DateTimeOffset DateTime;

        [JsonConverter(typeof(StringEnumConverter))]
        public EntityStatus Status { get; set; }
        public EntitySubstatus SubStatus { get; set; }
        public Dictionary<string, string> AditionalInfo { get; set; }
        public string BatchId { get; set; }
        public List<IntegrationError> Errors { get; set; } = new List<IntegrationError>();
        public IntegrationActionResult IntegrationActionResult { get; set; }

        #endregion
    }
}
