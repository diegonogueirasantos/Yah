namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement
{
    public class UpdateAnnoucementStatus
    {
        public string ItemId { get; set; }
        public AnnouncementStatus Status { get; set; }
        public string SubStatus { get; set; }
    }
}
