using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models.Order
{
    public class B2WDeliveryOrder : B2WOrderStatus
    {
        [JsonProperty("delivered_date")]
        public string delivered_date { get; set; }
    }
}

