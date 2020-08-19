using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class PostAdvertisementsModel
    {
        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public int? IntervalId { get; set; }
        public int? CampaignId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
    }
}
