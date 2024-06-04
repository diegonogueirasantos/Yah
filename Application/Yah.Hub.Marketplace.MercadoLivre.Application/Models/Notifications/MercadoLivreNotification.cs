using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Notifications
{
	public class MeliNotification
	{

            [JsonProperty("topic")]
            public string Topic { get; set; }

            [JsonProperty("_id")]
            public string Id { get; set; }

            [JsonProperty("resource")]
            public string Resource { get; set; }

            [JsonProperty("user_id")]
            public int UserId { get; set; }

            [JsonProperty("application_id")]
            public long ApplicationId { get; set; }

            [JsonProperty("attempts")]
            public int Attempts { get; set; }

            [JsonProperty("recieved")]
            public string Recieved { get; set; }

            [JsonProperty("sent")]
            public string Sent { get; set; }
        
    }
}

