using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Intervals
    {
        public Intervals()
        {
            Advertisementsintervals = new HashSet<Advertisementsintervals>();
        }

        public int IntervalId { get; set; }
        public string Title { get; set; }

        public virtual ICollection<Advertisementsintervals> Advertisementsintervals { get; set; }
    }
}
