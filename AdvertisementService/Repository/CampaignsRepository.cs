using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class CampaignsRepository : ICampaignsRepository
    {
        private readonly AdvertisementContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        private readonly AppSettings _appSettings;
        private ICommonFunctions _commonFunctions;
        public CampaignsRepository(IOptions<AppSettings> appSettings, AdvertisementContext context, IIncludeAdvertisementsRepository includeAdvertisements, ICommonFunctions commonFunctions)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
            _commonFunctions = commonFunctions;
        }

        public dynamic DeleteCampaigns(string id)
        {
            try
            {
                int campaignIdDecrypted = Obfuscation.Decode(id);
                var campaigns = _context.Campaigns.Include(x => x.Broadcasts).Where(x => x.CampaignId == campaignIdDecrypted).FirstOrDefault();
                if (campaigns == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                if (campaigns.Broadcasts != null)
                    _context.Broadcasts.RemoveRange(campaigns.Broadcasts);

                _context.Campaigns.Remove(campaigns);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.CampaignDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetAdvertisementsAsync(string campaignId, string advertisementsId, string includeType, string embed, string sort_by, Pagination pageInfo)
        {
            AdvertisementsGetResponse response = new AdvertisementsGetResponse();
            int totalCount = 0;
            try
            {
                List<string> campainIds = new List<string>();
                List<AdvertisementsGetModel> advertisementsModelList = new List<AdvertisementsGetModel>();
                if (string.IsNullOrEmpty(campaignId))
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                int campaignIdDecrypted = Obfuscation.Decode(campaignId);
                var campaignCount = _context.Campaigns.Where(x => x.CampaignId == campaignIdDecrypted).ToList().Count();
                if (campaignCount == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                if (string.IsNullOrEmpty(advertisementsId))
                {
                    var advertisements = _context.Advertisements.Include(x => x.Broadcasts).ToList();
                    var BroadcastsData = _context.AdvertisementsIntervals.ToList();
                    var campaignAdvertisement = _context.Broadcasts.Where(x => x.CampaignId == campaignIdDecrypted).ToList();
                    var advBasedOnCampaign = (from advertisement in advertisements
                                              join campAd in campaignAdvertisement on advertisement.AdvertisementId equals campAd.AdvertisementId into Details
                                              from m in Details.DefaultIfEmpty()
                                              select new Advertisements()
                                              {
                                                  AdvertisementId = advertisement.AdvertisementId,
                                                  InstitutionId = advertisement.InstitutionId,
                                                  ResourceNumber = advertisement.ResourceNumber,
                                                  Name = advertisement.Name,
                                                  CreatedAt = advertisement.CreatedAt,
                                                  MediaId = advertisement.MediaId,
                                                  Media = advertisement.Media,
                                                  Broadcasts = advertisement.Broadcasts,
                                                  AdvertisementsIntervals = advertisement.AdvertisementsIntervals,
                                                  TintColor = advertisement.TintColor,
                                                  InvertedTintColor = advertisement.InvertedTintColor,
                                              }).ToList();
                    var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advBasedOnCampaign, BroadcastsData, pageInfo);
                    advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                    totalCount = advertisements.Count();
                }
                else
                {
                    int advertisementsIdDecrypted = Obfuscation.Decode(advertisementsId);
                    var advertisements = _context.Advertisements.Include(x => x.Broadcasts).Where(x => x.AdvertisementId == advertisementsIdDecrypted).ToList();
                    var BroadcastsData = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == advertisementsIdDecrypted).ToList();
                    var campaignAdvertisement = _context.Broadcasts.Where(x => x.CampaignId == campaignIdDecrypted).ToList();
                    var advBasedOnCampaign = (from advertisement in advertisements
                                              join campAd in campaignAdvertisement on advertisement.AdvertisementId equals campAd.AdvertisementId into Details
                                              from m in Details.DefaultIfEmpty()
                                              select new Advertisements()
                                              {
                                                  AdvertisementId = advertisement.AdvertisementId,
                                                  InstitutionId = advertisement.InstitutionId,
                                                  ResourceNumber = advertisement.ResourceNumber,
                                                  Name = advertisement.Name,
                                                  CreatedAt = advertisement.CreatedAt,
                                                  MediaId = advertisement.MediaId,
                                                  Media = advertisement.Media,
                                                  Broadcasts = advertisement.Broadcasts,
                                                  AdvertisementsIntervals = advertisement.AdvertisementsIntervals,
                                                  TintColor = advertisement.TintColor,
                                                  InvertedTintColor = advertisement.InvertedTintColor,
                                              }).ToList();

                    var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advBasedOnCampaign, BroadcastsData, pageInfo);
                    advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                    totalCount = advertisements.Count();
                }

                if (!string.IsNullOrEmpty(embed) && embed.ToLower() == "sort")
                {
                    foreach (var item in advertisementsModelList)
                    {
                        int adsId = Obfuscation.Decode(item.AdvertisementId);
                        int campId = Obfuscation.Decode(item.CampaignId.FirstOrDefault());
                        item.Sort = _context.Broadcasts.Where(x => x.AdvertisementId == adsId && x.CampaignId == campId).Select(x => x.Sort).FirstOrDefault();
                    }
                }

                if (!string.IsNullOrEmpty(sort_by))
                {
                    var sortItem = sort_by.Split('.');
                    if (sortItem != null && !string.IsNullOrEmpty(sortItem.FirstOrDefault().ToLower()) && !string.IsNullOrEmpty(sortItem.LastOrDefault().ToLower()))
                    {
                        if (sortItem.LastOrDefault().ToLower() == "asc")
                        {
                            if (sortItem.FirstOrDefault().ToLower() == "advertisement" || sortItem.FirstOrDefault().ToLower() == "advertisements")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.AdvertisementId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "resourcename " || sortItem.FirstOrDefault().ToLower() == "resourcename")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.ResourceNumber).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "institution" || sortItem.FirstOrDefault().ToLower() == "institutions")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.InstitutionId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "media" || sortItem.FirstOrDefault().ToLower() == "medias")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.MediaId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "campaign" || sortItem.FirstOrDefault().ToLower() == "campaigns")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.CampaignId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "interval" || sortItem.FirstOrDefault().ToLower() == "intervals")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.IntervalId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "promotion" || sortItem.FirstOrDefault().ToLower() == "promotions")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.PromotionsId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "sort")
                                advertisementsModelList = advertisementsModelList.OrderBy(x => x.Sort).ToList();
                        }
                        else if (sortItem.LastOrDefault().ToLower() == "desc")
                        {
                            if (sortItem.FirstOrDefault().ToLower() == "advertisement" || sortItem.FirstOrDefault().ToLower() == "advertisements")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.AdvertisementId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "resourcename " || sortItem.FirstOrDefault().ToLower() == "resourcename")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.ResourceNumber).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "institution" || sortItem.FirstOrDefault().ToLower() == "institutions")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.InstitutionId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "media" || sortItem.FirstOrDefault().ToLower() == "medias")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.MediaId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "campaign" || sortItem.FirstOrDefault().ToLower() == "campaigns")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.CampaignId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "interval" || sortItem.FirstOrDefault().ToLower() == "intervals")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.IntervalId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "promotion" || sortItem.FirstOrDefault().ToLower() == "promotions")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.PromotionsId).ToList();
                            else if (sortItem.FirstOrDefault().ToLower() == "sort")
                                advertisementsModelList = advertisementsModelList.OrderByDescending(x => x.Sort).ToList();
                        }
                    }
                }

                dynamic includeData = new JObject();
                if (!string.IsNullOrEmpty(includeType) && advertisementsModelList.Count > 0)
                {
                    string[] includeArr = includeType.Split(',');
                    if (includeArr.Length > 0)
                    {
                        foreach (var item in includeArr)
                        {
                            if (item.ToLower() == "institution" || item.ToLower() == "institutions")
                            {
                                includeData.institution = _includeAdvertisements.GetInstitutionsIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "media" || item.ToLower() == "medias")
                            {
                                includeData.media = _includeAdvertisements.GetMediasIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "campaign" || item.ToLower() == "campaigns")
                            {
                                includeData.campaign = _includeAdvertisements.GetCampaignIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "interval" || item.ToLower() == "intervals")
                            {
                                includeData.interval = _includeAdvertisements.GetIntervalIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "promotion" || item.ToLower() == "promotions")
                            {
                                includeData.promotion = _includeAdvertisements.GetPromotionsForAdvertisementIncludedData(advertisementsModelList);
                            }
                        }
                    }
                }

                if (((JContainer)includeData).Count == 0)
                    includeData = null;

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.AdvertisementRetrived;
                response.included = includeData;
                response.pagination = page;
                response.data = advertisementsModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetCampaigns(string campaignId, string includeType, Pagination pageInfo)
        {
            int totalCount = 0;
            try
            {
                CampaignsGetResponse response = new CampaignsGetResponse();
                List<CampaignsModel> campaignsModelList = new List<CampaignsModel>();
                if (string.IsNullOrEmpty(campaignId))
                {
                    campaignsModelList = (from campaign in _context.Campaigns
                                          select new CampaignsModel()
                                          {
                                              CampaignId = Obfuscation.Encode(campaign.CampaignId),
                                              Title = campaign.Title,
                                              StartAt = campaign.StartAt,
                                              EndAt = campaign.EndAt,
                                              Status = campaign.Status,
                                              CreatedAt = campaign.CreatedAt,
                                              UpdatedAt = campaign.UpdatedAt
                                          }).AsEnumerable().OrderBy(a => a.CampaignId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Campaigns.ToList().Count();
                }
                else
                {
                    int campaignIdDecrypted = Obfuscation.Decode(campaignId);
                    campaignsModelList = (from campaign in _context.Campaigns
                                          where campaign.CampaignId == campaignIdDecrypted
                                          select new CampaignsModel()
                                          {
                                              CampaignId = Obfuscation.Encode(campaign.CampaignId),
                                              Title = campaign.Title,
                                              StartAt = campaign.StartAt,
                                              EndAt = campaign.EndAt,
                                              Status = campaign.Status
                                          }).AsEnumerable().OrderBy(a => a.CampaignId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Campaigns.Where(x => x.CampaignId == campaignIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.CampaignRetrived;
                response.pagination = page;
                response.data = campaignsModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic InsertCampaigns(CampaignsModel model)
        {
            try
            {
                Campaigns campaigns = new Campaigns()
                {
                    Title = model.Title,
                    StartAt = model.StartAt,
                    EndAt = model.EndAt,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Campaigns.Add(campaigns);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.CampaignInsert, true);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateCampaigns(CampaignsModel model)
        {
            try
            {
                int campaignIdDecrypted = Obfuscation.Decode(model.CampaignId);
                var campaignData = _context.Campaigns.Where(x => x.CampaignId == campaignIdDecrypted).FirstOrDefault();
                if (campaignData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                campaignData.StartAt = model.StartAt;
                campaignData.EndAt = model.EndAt;
                campaignData.Title = model.Title;
                campaignData.Status = model.Status;
                campaignData.UpdatedAt = DateTime.Now;
                _context.Campaigns.Update(campaignData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.CampaignUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic CreateBroadcasts(string campaignId, BroadcastsDto broadcastsDto)
        {
            if (string.IsNullOrEmpty(campaignId) || string.IsNullOrEmpty(broadcastsDto.AdvertisementId))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            int campaignIdDecoded = Obfuscation.Decode(campaignId);
            int advertisementIdDecoded = Obfuscation.Decode(broadcastsDto.AdvertisementId);

            return new Broadcasts
            {
                AdvertisementId = advertisementIdDecoded,
                CampaignId = campaignIdDecoded,
                Sort = broadcastsDto.Sort,
                CreatedAt = DateTime.Now
            };
        }

        public dynamic DeleteBroadcasts(string campaignId, string broadcastId)
        {
            if (string.IsNullOrEmpty(campaignId) || string.IsNullOrEmpty(broadcastId))
                throw new ArgumentNullException(CommonMessage.InvalidData);

            int campaignIdDecoded = Obfuscation.Decode(campaignId);
            int broadcastIdDecoded = Obfuscation.Decode(broadcastId);

            Broadcasts broadcast = _context.Broadcasts.Where(b => b.CampaignId == campaignIdDecoded && b.BroadcastId ==broadcastIdDecoded).FirstOrDefault();
            if (broadcast == null)
                throw new NullReferenceException(CommonMessage.BroadcastNotFound);

            return broadcast;
        }
    }
}