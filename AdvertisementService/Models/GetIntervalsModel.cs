using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class GetIntervalsModel
    {
        public int IntervalId { get; set; }
        public string Title { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
