using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvertisementService.Repository
{
    public class CampaignsRepository : ICampaignsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        public CampaignsRepository(advertisementserviceContext context, IIncludeAdvertisementsRepository includeAdvertisements)
        {
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public dynamic DeleteCampaigns(string id)
        {
            try
            {
                var campaigns = _context.Campaigns.Include(x => x.AdvertisementsCampaigns).Where(x => x.CampaignId == Convert.ToInt32(id)).FirstOrDefault();
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
                List<AdvertisementsModel> advertisementsModelList = new List<AdvertisementsModel>();
                if (campaignId == "0")
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var campaignCount = _context.Campaigns.Where(x => x.CampaignId == Convert.ToInt32(campaignId)).ToList().Count();
                if (campaignCount == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                if (advertisementsId == "0")
                {
                    advertisementsModelList = (from campaign in _context.Campaigns
                                               join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                               join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                               where campaign.CampaignId == Convert.ToInt32(campaignId)
                                               select new AdvertisementsModel()
                                               {
                                                   AdvertisementId = advertisement.AdvertisementId.ToString(),
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = advertisement.InstitutionId.ToString(),
                                                   MediaId = advertisement.MediaId.ToString(),
                                                   ResourceName = advertisement.ResourceName
                                               }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();


                    totalCount = (from campaign in _context.Campaigns
                                  join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                  join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                  where campaign.CampaignId == Convert.ToInt32(campaignId)
                                  select new AdvertisementsModel()
                                  {
                                      AdvertisementId = advertisement.AdvertisementId.ToString(),
                                      CreatedAt = advertisement.CreatedAt,
                                      InstitutionId = advertisement.InstitutionId.ToString(),
                                      MediaId = advertisement.MediaId.ToString(),
                                      ResourceName = advertisement.ResourceName
                                  }).ToList().Count();
                }
                else
                {
                    advertisementsModelList = (from campaign in _context.Campaigns
                                               join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                               join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                               where campaign.CampaignId == Convert.ToInt32(campaignId) && advertisement.AdvertisementId == Convert.ToInt32(advertisementsId)
                                               select new AdvertisementsModel()
                                               {
                                                   AdvertisementId = advertisement.AdvertisementId.ToString(),
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = advertisement.InstitutionId.ToString(),
                                                   MediaId = advertisement.MediaId.ToString(),
                                                   ResourceName = advertisement.ResourceName
                                               }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();


                    totalCount = (from campaign in _context.Campaigns
                                  join advertiseincampaign in _context.AdvertisementsCampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                  join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                  where campaign.CampaignId == Convert.ToInt32(campaignId) && advertisement.AdvertisementId == Convert.ToInt32(advertisementsId)
                                  select new AdvertisementsModel()
                                  {
                                      AdvertisementId = advertisement.AdvertisementId.ToString(),
                                      CreatedAt = advertisement.CreatedAt,
                                      InstitutionId = advertisement.InstitutionId.ToString(),
                                      MediaId = advertisement.MediaId.ToString(),
                                      ResourceName = advertisement.ResourceName
                                  }).ToList().Count();
                }

                if (advertisementsModelList == null || advertisementsModelList.Count == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                dynamic includeData = new JObject();
                if (!string.IsNullOrEmpty(includeType))
                {
                    string[] includeArr = includeType.Split(',');
                    if (includeArr.Length > 0)
                    {
                        foreach (var item in includeArr)
                        {
                            if (item.ToLower() == "institution")
                            {
                                includeData.institution = _includeAdvertisements.GetInstitutionsIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "media")
                            {
                                includeData.media = _includeAdvertisements.GetMediasIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "campaign")
                            {
                                includeData.campaign = _includeAdvertisements.GetCampaignIncludedData(advertisementsModelList);
                            }
                            else if (item.ToLower() == "interval")
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
                CampaignsGetResponse response = new CampaignsGetResponse();
                List<CampaignsModel> campaignsModelList = new List<CampaignsModel>();
                if (campaignId == "0")
                {
                    campaignsModelList = (from campaign in _context.Campaigns
                                          select new CampaignsModel()
                                          {
                                              CampaignId = campaign.CampaignId.ToString(),
                                              Title = campaign.Title,
                                              StartAt = campaign.StartAt,
                                              EndAt = campaign.EndAt,
                                              Status = campaign.Status
                                          }).OrderBy(a => a.CampaignId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Campaigns.ToList().Count();
                }
                else
                {
                    campaignsModelList = (from campaign in _context.Campaigns
                                          where campaign.CampaignId == Convert.ToInt32(campaignId)
                                          select new CampaignsModel()
                                          {
                                              CampaignId = campaign.CampaignId.ToString(),
                                              Title = campaign.Title,
                                              StartAt = campaign.StartAt,
                                              EndAt = campaign.EndAt,
                                              Status = campaign.Status
                                          }).OrderBy(a => a.CampaignId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Campaigns.Where(x => x.CampaignId == Convert.ToInt32(campaignId)).ToList().Count();
                }

                if (campaignsModelList == null || campaignsModelList.Count == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

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
                    Status = model.Status
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
                var campaignData = _context.Campaigns.Where(x => x.CampaignId == Convert.ToInt32(model.CampaignId)).FirstOrDefault();
                if (campaignData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                campaignData.StartAt = model.StartAt;
                campaignData.EndAt = model.EndAt;
                campaignData.Title = model.Title;
                campaignData.Status = model.Status;
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