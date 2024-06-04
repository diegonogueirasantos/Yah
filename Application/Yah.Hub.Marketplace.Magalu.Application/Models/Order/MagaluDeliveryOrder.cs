using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.Magalu.Application.Models.Order
{
    public class MagaluDeliveryOrder : MagaluOrderStatus
    {
        [JsonProperty("DeliveredDate")]
        public DateTimeOffset DeliveredDate { get; set; }
    }
}

