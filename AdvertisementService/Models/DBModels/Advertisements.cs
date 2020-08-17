using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Advertisements
    {
        public Advertisements()
        {
            Advertisementscampaigns = new HashSet<Advertisementscampaigns>();
            Advertisementsintervals = new HashSet<Advertisementsintervals>();
        }

        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }

        public virtual Medias Media { get; set; }
        public virtual ICollection<Advertisementscampaigns> Advertisementscampaigns { get; set; }
        public virtual ICollection<Advertisementsintervals> Advertisementsintervals { get; set; }
    }
}
