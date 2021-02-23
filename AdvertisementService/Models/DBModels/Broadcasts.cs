using System;

namespace AdvertisementService.Models.DBModels
{
    public partial class Broadcasts
    {
        public int BroadcastId { get; set; }
        public int AdvertisementId { get; set; }
        public int CampaignId { get; set; }
        public int? Sort { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Advertisements Advertisement { get; set; }
        public virtual Campaigns Campaign { get; set; }
    }
}
