using System;
namespace Yah.Hub.Common.ChannelConfiguration
{
	public class ExternalConfiguration
	{
		public string Uri { get; set; }
		public string Name { get; set; }
		public Dictionary<string, string> Headers { get; set; }
	}
}

