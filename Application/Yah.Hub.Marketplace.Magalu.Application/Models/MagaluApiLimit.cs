using Newtonsoft.Json;
using Yah.Hub.Common.Extensions;
using System.Text.RegularExpressions;

namespace Yah.Hub.Marketplace.Magalu.Application.Models
{
    public class MagaluApiLimit
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("RequestsByMinute")]
        public int RequestsByMinute { get; set; }

        [JsonProperty("RequestsByHour")]
        public int RequestsByHour { get; set; }
    }
}
