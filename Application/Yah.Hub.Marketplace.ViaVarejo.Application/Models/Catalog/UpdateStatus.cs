using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.ViaVarejo.Application.Models.Catalog
{
    public class UpdateStatus
    {
        public UpdateStatus(string sellerSkuId, SiteStatus[] siteStatus)
        {
            SellerSkuId = sellerSkuId;
            SiteStatus = siteStatus;
        }

        [JsonProperty("idSkuLojista")]
        public string SellerSkuId { get; set; }

        [JsonProperty("status")]
        public SiteStatus[] SiteStatus { get; set; }
    }

    public class SiteStatus
    {
        [JsonProperty("idSite")]
        public int SiteId { get; set; }

        [JsonProperty("status")]
        public char Status { get; set; }
    }
}
