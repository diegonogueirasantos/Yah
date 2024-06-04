using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models.Order
{
	public class B2WOrderStatus
	{
        [JsonProperty("status")]
        public string status { get; set; }

        [JsonIgnore]
        public string OrderId { get; set; }
    }
}

