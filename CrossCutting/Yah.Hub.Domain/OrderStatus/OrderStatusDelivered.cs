using System;
namespace Yah.Hub.Domain.OrderStatus
{
	public class OrderStatusDelivered: OrderStatus
	{
		public DateTimeOffset? DeliveryDate { get; set; }
	}
}

