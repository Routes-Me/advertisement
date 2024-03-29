﻿using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Helper.Functions;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoutesSecurity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AdvertisementService.Helper.Repository
{
    public class IncludeAdvertisementsRepository : IIncludeAdvertisementsRepository
    {
        private readonly AppSettings _appSettings;
        private readonly AdvertisementContext _context;
        private readonly Dependencies _dependencies;
        public IncludeAdvertisementsRepository(IOptions<AppSettings> appSettings, AdvertisementContext context, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _dependencies = dependencies.Value;
        }

        public dynamic GetCampaignIncludedData(List<AdvertisementsGetModel> advertisementsModel)
        {
            List<CampaignsModel> campaigns = new List<CampaignsModel>();
            foreach (var item in advertisementsModel)
            {
                var advertisementIdDecrypted = Obfuscation.Decode(item.AdvertisementId);
                var campaignsDetails = (from campaign in _context.Campaigns
                                        join campadvt in _context.Broadcasts on campaign.CampaignId equals campadvt.CampaignId
                                        join advt in _context.Advertisements on campadvt.AdvertisementId equals advt.AdvertisementId
                                        where advt.AdvertisementId == advertisementIdDecrypted
                                        select new CampaignsModel()
                                        {
                                            CampaignId = Obfuscation.Encode(campaign.CampaignId),
                                            StartAt = campaign.StartAt,
                                            EndAt = campaign.EndAt,
                                            Status = campaign.Status,
                                            Title = campaign.Title,
                                            CreatedAt = campaign.CreatedAt,
                                            UpdatedAt = campaign.UpdatedAt
                                        }).ToList().FirstOrDefault();

                if (campaignsDetails != null)
                    campaigns.Add(campaignsDetails);
            }
            var campaignsList = campaigns.GroupBy(x => x.CampaignId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(campaignsList.Cast<dynamic>().ToList());
        }

        public dynamic GetInstitutionsIncludedData(List<AdvertisementsGetModel> advertisementsModel)
        {
            List<InstitutionsModel> institutions = new List<InstitutionsModel>();
            foreach (var item in advertisementsModel)
            {
                var client = new RestClient(_appSettings.Host + _dependencies.InstitutionUrl + item.InstitutionId);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var institutionsData = JsonConvert.DeserializeObject<InstitutionGetResponse>(result);
                    institutions.AddRange(institutionsData.data);
                }
            }
            var institutionsList = institutions.GroupBy(x => x.InstitutionId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(institutionsList.Cast<dynamic>().ToList());
        }

        public dynamic GetIntervalIncludedData(List<AdvertisementsGetModel> advertisementsModel)
        {
            List<IntervalsModel> intervals = new List<IntervalsModel>();
            foreach (var item in advertisementsModel)
            {
                var advertisementIdDecrypted = Obfuscation.Decode(item.AdvertisementId);
                var intervalsDetails = (from interval in _context.Intervals
                                        join advtInterval in _context.AdvertisementsIntervals on interval.IntervalId equals advtInterval.IntervalId
                                        join advt in _context.Advertisements on advtInterval.AdvertisementId equals advt.AdvertisementId
                                        where advt.AdvertisementId == advertisementIdDecrypted
                                        select new IntervalsModel()
                                        {
                                            IntervalId = Obfuscation.Encode(interval.IntervalId),
                                            Title = interval.Title
                                        }).ToList().FirstOrDefault();
                if (intervalsDetails != null)
                    intervals.Add(intervalsDetails);

            }
            var intervalList = intervals.GroupBy(x => x.IntervalId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(intervalList.Cast<dynamic>().ToList());
        }

        public dynamic GetMediasIncludedData(List<AdvertisementsGetModel> advertisementsModel)
        {
            List<GetMediasModel> medias = new List<GetMediasModel>();
            foreach (var item in advertisementsModel)
            {
                var mediaIdDecrypted = Obfuscation.Decode(item.MediaId);
                var mediasDetails = (from media in _context.Medias
                                     join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                     where media.MediaId == mediaIdDecrypted
                                     select new GetMediasModel()
                                     {
                                         MediaId = Obfuscation.Encode(media.MediaId),
                                         CreatedAt = media.CreatedAt,
                                         Url = media.Url,
                                         MediaType = media.MediaType,
                                         Duration = metadata.Duration,
                                         Size = metadata.Size
                                     }).ToList().FirstOrDefault();
                if (mediasDetails != null)
                    medias.Add(mediasDetails);
            }
            var mediaList = medias.GroupBy(x => x.MediaId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(mediaList.Cast<dynamic>().ToList());
        }

        public dynamic GetPromotionsIncludedData(List<AdvertisementsForContentModel> advertisementsModelList)
        {
            List<PromotionsGetModel> promotions = new List<PromotionsGetModel>();
            foreach (var item in advertisementsModelList)
            {
                var client = new RestClient(_appSettings.Host + _dependencies.CouponsUrl + item.ContentId);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var promotionData = JsonConvert.DeserializeObject<PromotionsGetResponse>(result);
                    promotions.AddRange(promotionData.data);
                }
            }
            return promotions;
        }

        public List<PromotionsGetModel> GetPromotionsData()
        {
            List<PromotionsGetModel> promotions = new List<PromotionsGetModel>();
            var client = new RestClient(_appSettings.Host + _dependencies.PromotionsUrl);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content;
                var promotionData = JsonConvert.DeserializeObject<PromotionsGetResponse>(result);
                promotions.AddRange(promotionData.data);
            }
            return promotions;
        }

        public dynamic GetPromotionsForAdvertisementIncludedData(List<AdvertisementsGetModel> advertisementsModelList)
        {
            List<PromotionsGetModel> promotions = new List<PromotionsGetModel>();
            foreach (var item in advertisementsModelList)
            {
                if (!string.IsNullOrEmpty(item.PromotionsId))
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.PromotionsUrl + item.PromotionsId);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var promotionData = JsonConvert.DeserializeObject<PromotionsGetResponse>(result);
                        promotions.AddRange(promotionData.data);
                    }
                }
            }
            var promotionsList = promotions.GroupBy(x => x.PromotionId).Select(a => a.First()).ToList();
            return Common.SerializeJsonForIncludedRepo(promotionsList.Cast<dynamic>().ToList());
        }
    }
}
