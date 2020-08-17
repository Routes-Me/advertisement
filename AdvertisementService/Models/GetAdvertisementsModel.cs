using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class GetAdvertisementsModel
    {
        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
