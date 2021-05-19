using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Advertisements
    {
        public Advertisements()
        {
            Broadcasts = new HashSet<Broadcasts>();
            AdvertisementsIntervals = new HashSet<AdvertisementsIntervals>();
        }

        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public string ResourceNumber { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }

        public virtual Medias Media { get; set; }
        public virtual ICollection<Broadcasts> Broadcasts { get; set; }
        public virtual ICollection<AdvertisementsIntervals> AdvertisementsIntervals { get; set; }
    }
}
