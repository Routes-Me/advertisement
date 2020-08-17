using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Advertisementscampaigns
    {
        public int AdvertisementsCampaignsId { get; set; }
        public int? AdvertisementId { get; set; }
        public int? CampaignId { get; set; }

        public virtual Advertisements Advertisement { get; set; }
        public virtual Campaigns Campaign { get; set; }
    }
}
