namespace AdvertisementService.Models.DBModels
{
    public partial class AdvertisementsCampaigns
    {
        public int AdvertisementId { get; set; }
        public int CampaignId { get; set; }

        public virtual Advertisements Advertisement { get; set; }
        public virtual Campaigns Campaign { get; set; }
    }
}
