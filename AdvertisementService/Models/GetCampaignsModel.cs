using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class GetCampaignsModel
    {
        public int? CampaignId { get; set; }
        public int? AdvertisementId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }
        public bool isCampaign { get; set; } = false;
        public bool isAdvertisement { get; set; } = false;
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
