using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Obfuscation;

namespace AdvertisementService.Repository
{
    public class AdvertisementsRepository : IAdvertisementsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        private readonly AppSettings _appSettings;
        public AdvertisementsRepository(IOptions<AppSettings> appSettings, advertisementserviceContext context, IIncludeAdvertisementsRepository includeAdvertisements)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public dynamic DeleteAdvertisements(string id)
        {
            try
            {
                int advertisementIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(id), _appSettings.PrimeInverse);
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == advertisementIdDecrypted).FirstOrDefault();
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
                int institutionIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(institutionId), _appSettings.PrimeInverse);
                int advertisementIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(advertisementId), _appSettings.PrimeInverse);
                AdvertisementsGetResponse response = new AdvertisementsGetResponse();
                List<AdvertisementsGetModel> advertisementsModelList = new List<AdvertisementsGetModel>();
                if (institutionIdDecrypted == 0)
                {
                    if (advertisementIdDecrypted == 0)
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                                   join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                                   select new AdvertisementsGetModel()
                                                   {
                                                       AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                                                       MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                                                       ResourceName = advertisement.ResourceName,
                                                       CampaignId = ObfuscationClass.EncodeId(Convert.ToInt32(campaigns.CampaignId), _appSettings.Prime).ToString(),

                                                   }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = (from advertisement in _context.Advertisements
                                      join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                      join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                      select new AdvertisementsGetModel() { }).ToList().Count;
                    }
                    else
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                                   join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                                   where advertisement.AdvertisementId == advertisementIdDecrypted
                                                   select new AdvertisementsGetModel()
                                                   {
                                                       AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                                                       MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                                                       ResourceName = advertisement.ResourceName,
                                                       CampaignId = ObfuscationClass.EncodeId(Convert.ToInt32(campaigns.CampaignId), _appSettings.Prime).ToString(),
                                                   }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = (from advertisement in _context.Advertisements
                                      join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                      join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                      where advertisement.AdvertisementId == advertisementIdDecrypted
                                      select new AdvertisementsGetModel() { }).ToList().Count;
                    }
                }
                else
                {
                    if (advertisementIdDecrypted == 0)
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                                   join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                                   where advertisement.InstitutionId == institutionIdDecrypted
                                                   select new AdvertisementsGetModel()
                                                   {
                                                       AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                                                       MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                                                       ResourceName = advertisement.ResourceName,
                                                       CampaignId = ObfuscationClass.EncodeId(Convert.ToInt32(campaigns.CampaignId), _appSettings.Prime).ToString(),

                                                   }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = (from advertisement in _context.Advertisements
                                      join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                      join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                      where advertisement.InstitutionId == institutionIdDecrypted
                                      select new AdvertisementsGetModel() { }).ToList().Count();
                    }
                    else
                    {
                        advertisementsModelList = (from advertisement in _context.Advertisements
                                                   join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                                   join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                                   where advertisement.AdvertisementId == advertisementIdDecrypted && advertisement.InstitutionId == institutionIdDecrypted
                                                   select new AdvertisementsGetModel()
                                                   {
                                                       AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                                       CreatedAt = advertisement.CreatedAt,
                                                       InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                                                       MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                                                       ResourceName = advertisement.ResourceName,
                                                       CampaignId = ObfuscationClass.EncodeId(Convert.ToInt32(campaigns.CampaignId), _appSettings.Prime).ToString(),
                                                   }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                        totalCount = (from advertisement in _context.Advertisements
                                      join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                      join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                      where advertisement.AdvertisementId == advertisementIdDecrypted && advertisement.InstitutionId == institutionIdDecrypted
                                      select new AdvertisementsGetModel() { }).ToList().Count();
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

        public dynamic GetContents(string advertisementId, Pagination pageInfo, string token)
        {
            int totalCount = 0;
            try
            {
                int advertisementIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(advertisementId), _appSettings.PrimeInverse);
                ContentsGetResponse response = new ContentsGetResponse();
                List<AdvertisementsForContentModel> contentsModelList = new List<AdvertisementsForContentModel>();
                MediasModel medias = new MediasModel();
                List<ContentsModel> contents = new List<ContentsModel>();

                if (advertisementIdDecrypted == 0)
                {
                    contentsModelList = (from advertisement in _context.Advertisements
                                         join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                         join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                         join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                         where camp.Status == "active" && camp.StartAt < DateTime.Now && camp.EndAt > DateTime.Now
                                         select new AdvertisementsForContentModel()
                                         {
                                             ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                             Type = media.MediaType,
                                             Url = media.Url
                                         }).AsEnumerable().OrderBy(a => a.ContentId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from advertisement in _context.Advertisements
                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                  join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                  join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                  where camp.Status == "active" && camp.StartAt < DateTime.Now && camp.EndAt > DateTime.Now
                                  select new AdvertisementsForContentModel()
                                  {
                                      ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString()
                                  }).AsEnumerable().ToList().Count();
                }
                else
                {
                    contentsModelList = (from advertisement in _context.Advertisements
                                         join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                         join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                         join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                         where camp.Status == "active" && camp.StartAt < DateTime.Now && camp.EndAt > DateTime.Now && advertisement.AdvertisementId == advertisementIdDecrypted
                                         select new AdvertisementsForContentModel()
                                         {
                                             ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                             Type = media.MediaType,
                                             Url = media.Url
                                         }).AsEnumerable().OrderBy(a => a.ContentId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from advertisement in _context.Advertisements
                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                  join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                  join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                  where camp.Status == "active" && camp.StartAt < DateTime.Now && camp.EndAt > DateTime.Now && advertisement.AdvertisementId == advertisementIdDecrypted
                                  select new AdvertisementsForContentModel()
                                  {
                                      ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString()
                                  }).AsEnumerable().ToList().Count();
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


                if (contentsModelList.Count > 0)
                {
                    List<PromotionsGetModel> promotions = _includeAdvertisements.GetPromotionsIncludedData(contentsModelList);
                    if (promotions != null && promotions.Count > 0)
                    {
                        foreach (var content in contents)
                        {
                            foreach (var promotion in promotions)
                            {
                                if (content.ContentId == promotion.AdvertisementId)
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
                var media = _context.Medias.Where(x => x.MediaId == ObfuscationClass.DecodeId(Convert.ToInt32(model.MediaId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (media == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var interval = _context.Intervals.Where(x => x.IntervalId == ObfuscationClass.DecodeId(Convert.ToInt32(model.IntervalId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (interval == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == ObfuscationClass.DecodeId(Convert.ToInt32(model.CampaignId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                Advertisements advertisements = new Advertisements()
                {
                    CreatedAt = DateTime.UtcNow,
                    InstitutionId = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse),
                    MediaId = ObfuscationClass.DecodeId(Convert.ToInt32(model.MediaId), _appSettings.PrimeInverse),
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
                response.AdvertisementId = ObfuscationClass.EncodeId(advertisements.AdvertisementId, _appSettings.Prime).ToString();
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
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == ObfuscationClass.DecodeId(Convert.ToInt32(model.AdvertisementId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (advertisements == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var media = _context.Medias.Where(x => x.MediaId == ObfuscationClass.DecodeId(Convert.ToInt32(model.MediaId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (media == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var interval = _context.Intervals.Where(x => x.IntervalId == ObfuscationClass.DecodeId(Convert.ToInt32(model.IntervalId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (interval == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == ObfuscationClass.DecodeId(Convert.ToInt32(model.CampaignId), _appSettings.PrimeInverse)).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                var advertisementsinterval = advertisements.AdvertisementsIntervals.Where(x => x.AdvertisementId == ObfuscationClass.DecodeId(Convert.ToInt32(model.AdvertisementId), _appSettings.PrimeInverse)).FirstOrDefault();
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

                var advertisementscampaigns = advertisements.AdvertisementsCampaigns.Where(x => x.AdvertisementId == ObfuscationClass.DecodeId(Convert.ToInt32(model.AdvertisementId), _appSettings.PrimeInverse)).FirstOrDefault();
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

                advertisements.InstitutionId = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse);
                advertisements.MediaId = ObfuscationClass.DecodeId(Convert.ToInt32(model.MediaId), _appSettings.PrimeInverse);
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