using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
	public class MagaluOrderStatus
	{
        [JsonProperty("IdOrder")]
        public string IdOrder { get; set; }

        [JsonProperty("OrderStatus")]
        public string Status { get; set; }
    }
}

