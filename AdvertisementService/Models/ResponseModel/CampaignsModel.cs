﻿using System;

namespace AdvertisementService.Models.ResponseModel
{
    public class CampaignsModel
    {
        public int CampaignId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }
    }
}
