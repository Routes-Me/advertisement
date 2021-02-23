using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Campaigns
    {
        public Campaigns()
        {
            Broadcasts = new HashSet<Broadcasts>();
        }

        public int CampaignId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Broadcasts> Broadcasts { get; set; }
    }
}
