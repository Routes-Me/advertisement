using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class AdvertisementsIntervals
    {
        public int IntervalId { get; set; }
        public int AdvertisementId { get; set; }

        public virtual Advertisements Advertisement { get; set; }
        public virtual Intervals Interval { get; set; }
    }
}
