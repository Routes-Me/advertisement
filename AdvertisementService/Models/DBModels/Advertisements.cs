using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Advertisements
    {
        public Advertisements()
        {
            AdvertisementsCampaigns = new HashSet<AdvertisementsCampaigns>();
            AdvertisementsIntervals = new HashSet<AdvertisementsIntervals>();
        }

        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public string ResourceName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }

        public virtual Medias Media { get; set; }
        public virtual ICollection<AdvertisementsCampaigns> AdvertisementsCampaigns { get; set; }
        public virtual ICollection<AdvertisementsIntervals> AdvertisementsIntervals { get; set; }
    }
}
