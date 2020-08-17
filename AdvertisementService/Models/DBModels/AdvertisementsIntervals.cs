using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Advertisementsintervals
    {
        public int AdvertisementsIntervalsId { get; set; }
        public int? IntervalId { get; set; }
        public int? AdvertisementId { get; set; }

        public virtual Advertisements Advertisement { get; set; }
        public virtual Intervals Interval { get; set; }
    }
}
