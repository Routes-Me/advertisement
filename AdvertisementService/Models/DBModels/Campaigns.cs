using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Campaigns
    {
        public Campaigns()
        {
            Advertisementscampaigns = new HashSet<Advertisementscampaigns>();
        }

        public int CampaignId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }

        public virtual ICollection<Advertisementscampaigns> Advertisementscampaigns { get; set; }
    }
}
