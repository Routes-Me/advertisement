using System;

namespace AdvertisementService.Models.ResponseModel
{
    public class CampaignsModel
    {
        public string CampaignId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
