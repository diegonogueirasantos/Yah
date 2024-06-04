using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.B2W.Application.Models
{
	public class ProductLink
    {
        [JsonProperty("skus", Required = Required.Always)]
        public List<string> Skus { get; set; }

        [JsonProperty("sale_system", Required = Required.Always)]
        public string SaleSystem { get; private set; } = "B2W";

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; } = "link";
    }

	public class ProductLinkResult
	{
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }
	}


    public enum SaleSystem
    {
        B2W,
        Americanas
    }
}

