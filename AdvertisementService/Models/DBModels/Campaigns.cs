using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Campaigns
    {
        public Campaigns()
        {
            AdvertisementsCampaigns = new HashSet<AdvertisementsCampaigns>();
        }

        public int CampaignId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }

        public virtual ICollection<AdvertisementsCampaigns> AdvertisementsCampaigns { get; set; }
    }
}
