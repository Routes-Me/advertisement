using AdvertisementService.Models.DBModels;
using Newtonsoft.Json;
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
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SortIndex { get; set; }
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
        public int? SortIndex { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
    }

    public class PostAdvertisementsModel
    {
        public string AdvertisementId { get; set; }
        public string ResourceName { get; set; }
        public string InstitutionId { get; set; }
        public string IntervalId { get; set; }
        public List<string> CampaignId { get; set; }
        public string MediaUrl { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
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
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
        public PromotionsModelForContent promotion { get; set; }

    }

    public class PatchSort
    {
        public int? Sort { get; set; }
    }

    public class PatchSortList
    {
        public List<PatchSortListItem> SortItem{ get; set; }
    }

    public class PatchSortListItem
    {
        public string AdvertisementId { get; set; }
        public int Sort { get; set; }
    }

    public class AdvertisementsGetModelWithCampaign
    {
        public string AdvertisementId { get; set; }
        public string ResourceName { get; set; }
        public string InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaId { get; set; }
        public List<Broadcasts> Campaigns { get; set; }
        public string IntervalId { get; set; }
        public string PromotionsId { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
    }
}
