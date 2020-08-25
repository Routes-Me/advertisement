using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Intervals
    {
        public Intervals()
        {
            AdvertisementsIntervals = new HashSet<AdvertisementsIntervals>();
        }

        public int IntervalId { get; set; }
        public string Title { get; set; }

        public virtual ICollection<AdvertisementsIntervals> AdvertisementsIntervals { get; set; }
    }
}
