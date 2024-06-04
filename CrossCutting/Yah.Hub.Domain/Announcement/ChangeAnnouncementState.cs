using System;
namespace Yah.Hub.Domain.Announcement
{
	public class ChangeAnnouncementState
	{
		public AnnouncementState State {get;set;}
		public string AnnouncementId { get; set; }
    }
}

