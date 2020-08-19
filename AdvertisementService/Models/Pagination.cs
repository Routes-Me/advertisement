using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class Pagination
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class PageInfo
    {
        public int currentPage { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
