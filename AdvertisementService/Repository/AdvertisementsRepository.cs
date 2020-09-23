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

        public dynamic DeleteAdvertisements(int id)
        {
            try
            {
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == id).FirstOrDefault();
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

        public dynamic GetAdvertisements(int institutionId, int advertisementId, string includeType, Pagination pageInfo)
        {
            int totalCount = 0;
            try
            {
                AdvertisementsGetResponse response = new AdvertisementsGetResponse();
                List<AdvertisementsModel> advertisementsModelList = new List<AdvertisementsModel>();
                if (institutionId == 0)
                {
                    if (advertisementId == 0)
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId,
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId,
                                                       MediaId = advertisement.MediaId,
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.ToList().Count();
                    }
                    else
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   where advertisement.AdvertisementId == advertisementId
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId,
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId,
                                                       MediaId = advertisement.MediaId,
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.Where(x => x.AdvertisementId == advertisementId).ToList().Count();
                    }

                }
                else
                {
                    if (advertisementId == 0)
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   where advertisement.InstitutionId == institutionId
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId,
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId,
                                                       MediaId = advertisement.MediaId,
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.Where(x => x.InstitutionId == institutionId).ToList().Count();
                    }
                    else
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   where advertisement.AdvertisementId == advertisementId && advertisement.InstitutionId == institutionId
                                                   select new AdvertisementsModel()
                                                   {
                                                       AdvertisementId = advertisement.AdvertisementId,
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = advertisement.InstitutionId,
                                                       MediaId = advertisement.MediaId,
                                                       ResourceName = advertisement.ResourceName
                                                   }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = _context.Advertisements.Where(x => x.AdvertisementId == advertisementId && x.InstitutionId == institutionId).ToList().Count();
                    }
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

        public dynamic InsertAdvertisements(PostAdvertisementsModel model)
        {
            try
            {
                var media = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (media == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var interval = _context.Intervals.Where(x => x.IntervalId == model.IntervalId).FirstOrDefault();
                if (interval == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                Advertisements advertisements = new Advertisements()
                {
                    CreatedAt = DateTime.UtcNow,
                    InstitutionId = model.InstitutionId,
                    MediaId = model.MediaId,
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
                return ReturnResponse.SuccessResponse(CommonMessage.AdvertisementInsert, true);
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
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
                if (advertisements == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var media = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (media == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var interval = _context.Intervals.Where(x => x.IntervalId == model.IntervalId).FirstOrDefault();
                if (interval == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                var advertisementsinterval = advertisements.AdvertisementsIntervals.Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
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

                var advertisementscampaigns = advertisements.AdvertisementsCampaigns.Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
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

                advertisements.InstitutionId = model.InstitutionId;
                advertisements.MediaId = model.MediaId;
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