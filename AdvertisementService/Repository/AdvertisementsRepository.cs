using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvertisementService.Repository
{
    public class AdvertisementsRepository : IAdvertisementsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        public AdvertisementsRepository(advertisementserviceContext context, IIncludeAdvertisementsRepository includeAdvertisements)
        {
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public dynamic DeleteAdvertisements(string id)
        {
            try
            {
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == Convert.ToInt32(id)).FirstOrDefault();
                if (advertisements == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                if (advertisements.AdvertisementsIntervals != null)
                    _context.AdvertisementsIntervals.RemoveRange(advertisements.AdvertisementsIntervals);

                if (advertisements.AdvertisementsCampaigns != null)
                    _context.AdvertisementsCampaigns.RemoveRange(advertisements.AdvertisementsCampaigns);

                _context.Advertisements.Remove(advertisements);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.AdvertisementDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetAdvertisements(string institutionId, string advertisementId, string includeType, Pagination pageInfo)
        {
            int totalCount = 0;
            try
            {
                AdvertisementsGetResponse response = new AdvertisementsGetResponse();
                List<AdvertisementsModel> advertisementsModelList = new List<AdvertisementsModel>();
                if (institutionId == "0")
                {
                    if (advertisementId == "0")
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId.ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId.ToString(),
                                                       MediaId = advertisement.MediaId.ToString(),
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.ToList().Count();
                    }
                    else
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   where advertisement.AdvertisementId == Convert.ToInt32(advertisementId)
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId.ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId.ToString(),
                                                       MediaId = advertisement.MediaId.ToString(),
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.Where(x => x.AdvertisementId == Convert.ToInt32(advertisementId)).ToList().Count();
                    }

                }
                else
                {
                    if (advertisementId == "0")
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   where advertisement.InstitutionId == Convert.ToInt32(institutionId)
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId.ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId.ToString(),
                                                       MediaId = advertisement.MediaId.ToString(),
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.Where(x => x.InstitutionId == Convert.ToInt32(institutionId)).ToList().Count();
                    }
                    else
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   where advertisement.AdvertisementId == Convert.ToInt32(advertisementId) && advertisement.InstitutionId == Convert.ToInt32(institutionId)
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId.ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId.ToString(),
                                                       MediaId = advertisement.MediaId.ToString(),
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.Where(x => x.AdvertisementId == Convert.ToInt32(advertisementId) && x.InstitutionId == Convert.ToInt32(institutionId)).ToList().Count();
                    }
                }

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

        public dynamic GetContents(string advertisementId, Pagination pageInfo)
        {
            int totalCount = 0;
            try
            {
                ContentsGetResponse response = new ContentsGetResponse();
                List<AdvertisementsForContentModel> contentsModelList = new List<AdvertisementsForContentModel>();
                MediasModel medias = new MediasModel();
                List<ContentsModel> contents = new List<ContentsModel>();

                if (advertisementId == "0")
                {
                    contentsModelList = (from advertisement in _context.Advertisements
                                         join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                         select new AdvertisementsForContentModel()
                                         {
                                             ContentId = advertisement.AdvertisementId.ToString(),
                                             Type = media.MediaType,
                                             Url = media.Url
                                         }).OrderBy(a => a.ContentId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Advertisements.ToList().Count();
                }
                else
                {
                    contentsModelList = (from advertisement in _context.Advertisements
                                         join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                         where advertisement.AdvertisementId == Convert.ToInt32(advertisementId)
                                         select new AdvertisementsForContentModel()
                                         {
                                             ContentId = advertisement.AdvertisementId.ToString(),
                                             Type = media.MediaType,
                                             Url = media.Url
                                         }).OrderBy(a => a.ContentId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Advertisements.Where(x => x.AdvertisementId == Convert.ToInt32(advertisementId)).ToList().Count();
                }


                foreach (var content in contentsModelList)
                {
                    ContentsModel contentsModel = new ContentsModel()
                    {
                        ContentId = content.ContentId,
                        Type = content.Type,
                        Url = content.Url
                    };
                    contents.Add(contentsModel);
                }

                List<PromotionsModel> promotions = _includeAdvertisements.GetPromotionsIncludedData(contentsModelList);

                if (promotions != null && promotions.Count > 0)
                {
                    foreach (var content in contents)
                    {
                        foreach (var promotion in promotions)
                        {
                            if (content.ContentId == promotion.PromotionId)
                            {
                                content.promotion = new PromotionsModel()
                                {
                                    Title = promotion.Title,
                                    Subtitle = promotion.Subtitle,
                                    PromotionId = promotion.PromotionId,
                                    LogoUrl = promotion.LogoUrl
                                };
                            }
                        }
                    }
                }
                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.ContentsRetrive;
                response.pagination = page;
                response.data = contents;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic InsertAdvertisements(PostAdvertisementsModel model)
        {
            AdvertisementsPostResponse response = new AdvertisementsPostResponse();
            try
            {
                var media = _context.Medias.Where(x => x.MediaId == Convert.ToInt32(model.MediaId)).FirstOrDefault();
                if (media == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var interval = _context.Intervals.Where(x => x.IntervalId == Convert.ToInt32(model.IntervalId)).FirstOrDefault();
                if (interval == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == Convert.ToInt32(model.CampaignId)).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                Advertisements advertisements = new Advertisements()
                {
                    CreatedAt = DateTime.UtcNow,
                    InstitutionId = Convert.ToInt32(model.InstitutionId),
                    MediaId = Convert.ToInt32(model.MediaId),
                    ResourceName = model.ResourceName
                };
                _context.Advertisements.Add(advertisements);
                _context.SaveChanges();

                AdvertisementsIntervals advertisementsintervals = new AdvertisementsIntervals()
                {
                    AdvertisementId = advertisements.AdvertisementId,
                    IntervalId = interval.IntervalId
                };
                _context.AdvertisementsIntervals.Add(advertisementsintervals);

                AdvertisementsCampaigns objAdvertisementscampaigns = new AdvertisementsCampaigns()
                {
                    AdvertisementId = advertisements.AdvertisementId,
                    CampaignId = campaign.CampaignId
                };
                _context.AdvertisementsCampaigns.Add(objAdvertisementscampaigns);
                _context.SaveChanges();

                response.status = true;
                response.statusCode = StatusCodes.Status201Created;
                response.message = CommonMessage.AdvertisementInsert;
                response.AdvertisementId = advertisements.AdvertisementId.ToString();
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateAdvertisements(PostAdvertisementsModel model)
        {
            try
            {
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == Convert.ToInt32(model.AdvertisementId)).FirstOrDefault();
                if (advertisements == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var media = _context.Medias.Where(x => x.MediaId == Convert.ToInt32(model.MediaId)).FirstOrDefault();
                if (media == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var interval = _context.Intervals.Where(x => x.IntervalId == Convert.ToInt32(model.IntervalId)).FirstOrDefault();
                if (interval == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == Convert.ToInt32(model.CampaignId)).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                var advertisementsinterval = advertisements.AdvertisementsIntervals.Where(x => x.AdvertisementId == Convert.ToInt32(model.AdvertisementId)).FirstOrDefault();
                if (advertisementsinterval == null)
                {
                    AdvertisementsIntervals objAdvertisementsintervals = new AdvertisementsIntervals()
                    {
                        AdvertisementId = advertisements.AdvertisementId,
                        IntervalId = interval.IntervalId
                    };
                    _context.AdvertisementsIntervals.Add(advertisementsinterval);
                }
                else
                {
                    advertisementsinterval.AdvertisementId = advertisements.AdvertisementId;
                    advertisementsinterval.IntervalId = interval.IntervalId;
                    _context.AdvertisementsIntervals.Update(advertisementsinterval);
                }

                var advertisementscampaigns = advertisements.AdvertisementsCampaigns.Where(x => x.AdvertisementId == Convert.ToInt32(model.AdvertisementId)).FirstOrDefault();
                if (advertisementscampaigns == null)
                {
                    AdvertisementsCampaigns objAdvertisementsintervals = new AdvertisementsCampaigns()
                    {
                        CampaignId = campaign.CampaignId,
                        AdvertisementId = advertisements.AdvertisementId
                    };
                    _context.AdvertisementsCampaigns.Add(objAdvertisementsintervals);
                }
                else
                {
                    advertisementscampaigns.AdvertisementId = advertisements.AdvertisementId;
                    advertisementscampaigns.CampaignId = campaign.CampaignId;
                    _context.AdvertisementsCampaigns.Update(advertisementscampaigns);
                }

                advertisements.InstitutionId = Convert.ToInt32(model.InstitutionId);
                advertisements.MediaId = Convert.ToInt32(model.MediaId);
                advertisements.ResourceName = model.ResourceName;
                _context.Advertisements.Update(advertisements);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.AdvertisementUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}