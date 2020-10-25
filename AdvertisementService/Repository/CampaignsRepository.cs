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
using Obfuscation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvertisementService.Repository
{
    public class CampaignsRepository : ICampaignsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements; 
        private readonly AppSettings _appSettings;
        public CampaignsRepository(IOptions<AppSettings> appSettings, advertisementserviceContext context, IIncludeAdvertisementsRepository includeAdvertisements)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public dynamic DeleteCampaigns(string id)
        {
            try
            {
                int campaignIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(id), _appSettings.PrimeInverse);
                var campaigns = _context.Campaigns.Include(x => x.AdvertisementsCampaigns).Where(x => x.CampaignId == campaignIdDecrypted).FirstOrDefault();
                if (campaigns == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                if (campaigns.AdvertisementsCampaigns != null)
                    _context.AdvertisementsCampaigns.RemoveRange(campaigns.AdvertisementsCampaigns);

                _context.Campaigns.Remove(campaigns);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.CampaignDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetAdvertisements(string campaignId, string advertisementsId, string includeType, Pagination pageInfo)
        {
            AdvertisementsGetResponse response = new AdvertisementsGetResponse();
            int totalCount = 0;
            try
            {
                List<string> campainIds = new List<string>();
                int campaignIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(campaignId), _appSettings.PrimeInverse);
                int advertisementsIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(advertisementsId), _appSettings.PrimeInverse);
                List<AdvertisementsGetModel> advertisementsModelList = new List<AdvertisementsGetModel>();
                if (campaignIdDecrypted == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var campaignCount = _context.Campaigns.Where(x => x.CampaignId == campaignIdDecrypted).ToList().Count();
                if (campaignCount == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                if (advertisementsIdDecrypted == 0)
                {
                    var campaignList = (from advertisement in _context.Advertisements
                                        join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                        join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                        where campaigns.CampaignId == campaignIdDecrypted
                                        select new GetCampaignList()
                                        {
                                            campaigns = campaigns
                                        }).ToList();

                    foreach (var item in campaignList)
                    {
                        string Ids = ObfuscationClass.EncodeId(Convert.ToInt32(item.campaigns.CampaignId), _appSettings.Prime).ToString();
                        campainIds.Add(Ids);
                    }

                    advertisementsModelList = (from campaign in _context.Campaigns
                                               join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                               join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                               join advertisementsIntervals in _context.AdvertisementsIntervals on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId
                                               join intervals in _context.Intervals on advertisementsIntervals.IntervalId equals intervals.IntervalId
                                               where campaign.CampaignId == campaignIdDecrypted
                                               select new AdvertisementsGetModel()
                                               {
                                                   AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                                                   MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                                                   ResourceName = advertisement.ResourceName,
                                                   CampaignId = campainIds,
                                                   IntervalId = ObfuscationClass.EncodeId(Convert.ToInt32(intervals.IntervalId), _appSettings.Prime).ToString(),
                                               }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();


                    totalCount = (from campaign in _context.Campaigns
                                  join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                  join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                  join advertisementsIntervals in _context.AdvertisementsIntervals on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId
                                  join intervals in _context.Intervals on advertisementsIntervals.IntervalId equals intervals.IntervalId
                                  where campaign.CampaignId == campaignIdDecrypted
                                  select new AdvertisementsGetModel(){}).AsEnumerable().ToList().Count();
                }
                else
                {
                    var campaignList = (from advertisement in _context.Advertisements
                                        join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                        join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                        where campaigns.CampaignId == campaignIdDecrypted && advertisement.AdvertisementId == advertisementsIdDecrypted
                                        select new GetCampaignList()
                                        {
                                            campaigns = campaigns
                                        }).ToList();

                    foreach (var item in campaignList)
                    {
                        string Ids = ObfuscationClass.EncodeId(Convert.ToInt32(item.campaigns.CampaignId), _appSettings.Prime).ToString();
                        campainIds.Add(Ids);
                    }
                    advertisementsModelList = (from campaign in _context.Campaigns
                                               join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                               join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                               join advertisementsIntervals in _context.AdvertisementsIntervals on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId
                                               join intervals in _context.Intervals on advertisementsIntervals.IntervalId equals intervals.IntervalId
                                               where campaign.CampaignId == campaignIdDecrypted && advertisement.AdvertisementId == advertisementsIdDecrypted
                                               select new AdvertisementsGetModel()
                                               {
                                                   AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                                                   MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                                                   ResourceName = advertisement.ResourceName,
                                                   CampaignId = campainIds,
                                                   IntervalId = ObfuscationClass.EncodeId(Convert.ToInt32(intervals.IntervalId), _appSettings.Prime).ToString(),
                                               }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();


                    totalCount = (from campaign in _context.Campaigns
                                  join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                  join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                  join advertisementsIntervals in _context.AdvertisementsIntervals on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId
                                  join intervals in _context.Intervals on advertisementsIntervals.IntervalId equals intervals.IntervalId
                                  where campaign.CampaignId == campaignIdDecrypted && advertisement.AdvertisementId == advertisementsIdDecrypted
                                  select new AdvertisementsGetModel(){}).AsEnumerable().ToList().Count();
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
                int campaignIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(campaignId), _appSettings.PrimeInverse);
                CampaignsGetResponse response = new CampaignsGetResponse();
                List<CampaignsModel> campaignsModelList = new List<CampaignsModel>();
                if (campaignIdDecrypted == 0)
                {
                    campaignsModelList = (from campaign in _context.Campaigns
                                          select new CampaignsModel()
                                          {
                                              CampaignId = ObfuscationClass.EncodeId(campaign.CampaignId, _appSettings.Prime).ToString(),
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
                    campaignsModelList = (from campaign in _context.Campaigns
                                          where campaign.CampaignId == campaignIdDecrypted
                                          select new CampaignsModel()
                                          {
                                              CampaignId = ObfuscationClass.EncodeId(campaign.CampaignId, _appSettings.Prime).ToString(),
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
                int campaignIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(model.CampaignId), _appSettings.PrimeInverse);
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
    }
}