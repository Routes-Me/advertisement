using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.ResponseModel
{
    public class AdvertisementsModel
    {
        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public int? IntervalId { get; set; }
        public int? CampaignId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
        public string Url { get; set; }
        public string MediaType { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; }
        public string IntervalTitle { get; set; }
    }
}
