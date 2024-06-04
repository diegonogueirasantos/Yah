using System;
using Yah.Hub.Common.Enums;

namespace Yah.Hub.Common.Notification
{
	public class NotificationEvent<T> 
	{
		public DateTime EventDateTime { get; set; }
		public EntityType EntityType { get; set; }
		public T Data { get; set; }
	}
}

