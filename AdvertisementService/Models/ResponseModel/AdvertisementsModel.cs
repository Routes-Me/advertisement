using AdvertisementService.Models.DBModels;
using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.ResponseModel
{
    public class AdvertisementsModel
    {
        public string AdvertisementId { get; set; }
        public string ResourceName { get; set; }
        public string InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaId { get; set; }
    }

    public class AdvertisementsGetModel
    {
        public string AdvertisementId { get; set; }
        public string ResourceName { get; set; }
        public string InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaId { get; set; }
        public List<string> CampaignId { get; set; }
        public string IntervalId { get; set; }
        public string PromotionsId { get; set; }
    }
    public class GetCampaignList
    {
        public string campaignId { get; set; }
    }




    public class AdvertisementsForContentModel
    {
        public string ContentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class PostAdvertisementsModel
    {
        public string AdvertisementId { get; set; }
        public string ResourceName { get; set; }
        public string InstitutionId { get; set; }
        public string IntervalId { get; set; }
        public List<string> CampaignId { get; set; }
        public string MediaId { get; set; }
    }

    public class GetActiveCampAdModel
    {
        public string ContentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class ContentsModel
    {
        public string ContentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public PromotionsModel promotion { get; set; }

    }
}
