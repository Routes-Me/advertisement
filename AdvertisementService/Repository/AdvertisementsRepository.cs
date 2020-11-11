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
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Hosting;

namespace AdvertisementService.Repository
{
    public class AdvertisementsRepository : IAdvertisementsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        private readonly AppSettings _appSettings;
        private IWebHostEnvironment _hostingEnv;
        private ICommonFunctions _commonFunctions;

        public AdvertisementsRepository(IOptions<AppSettings> appSettings, advertisementserviceContext context, IIncludeAdvertisementsRepository includeAdvertisements, IWebHostEnvironment hostingEnv, ICommonFunctions commonFunctions)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
            _hostingEnv = hostingEnv;
            _commonFunctions = commonFunctions;
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
                List<string> campainIds = new List<string>();

                if (institutionIdDecrypted == 0)
                {
                    if (advertisementIdDecrypted == 0)
                    {
                        var advertisements = _context.Advertisements.Include(x => x.AdvertisementsCampaigns).ToList();
                        var advertisementsCampaignsData = _context.AdvertisementsIntervals.ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, advertisementsCampaignsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                    else
                    {
                        var advertisements = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted).ToList();
                        var advertisementsCampaignsData = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == advertisementIdDecrypted).ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, advertisementsCampaignsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                }
                else
                {
                    if (advertisementIdDecrypted == 0)
                    {
                        var advertisements = _context.Advertisements.Where(x => x.InstitutionId == institutionIdDecrypted).ToList();
                        var advertisementsCampaignsData = _context.AdvertisementsIntervals.Where(x => x.Advertisement.InstitutionId == institutionIdDecrypted).ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, advertisementsCampaignsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                    else
                    {
                        var advertisements = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.InstitutionId == institutionIdDecrypted).ToList();
                        var advertisementsCampaignsData = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.Advertisement.InstitutionId == institutionIdDecrypted).ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, advertisementsCampaignsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
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

        public dynamic GetContents(string advertisementId, Pagination pageInfo)
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
                                         where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now
                                         select new AdvertisementsForContentModel()
                                         {
                                             ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                             Type = media.MediaType,
                                             Url = media.Url,
                                             SortIndex = advtcamp.SortIndex,
                                             TintColor = advertisement.TintColor,
                                             InvertedTintColor = advertisement.InvertedTintColor
                                         }).AsEnumerable().GroupBy(x => x.ContentId).Select(a => a.First()).OrderBy(a => a.SortIndex)
                                         .Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from advertisement in _context.Advertisements
                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                  join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                  join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                  where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now
                                  select new AdvertisementsForContentModel()
                                  {
                                      ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString()
                                  }).AsEnumerable().GroupBy(x => x.ContentId).Select(a => a.First()).ToList().Count();
                }
                else
                {
                    contentsModelList = (from advertisement in _context.Advertisements
                                         join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                         join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                         join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                         where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now && advertisement.AdvertisementId == advertisementIdDecrypted
                                         select new AdvertisementsForContentModel()
                                         {
                                             ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                                             Type = media.MediaType,
                                             Url = media.Url,
                                             SortIndex = advtcamp.SortIndex,
                                             TintColor = advertisement.TintColor,
                                             InvertedTintColor = advertisement.InvertedTintColor
                                         }).AsEnumerable().GroupBy(x => x.ContentId).Select(a => a.First()).OrderBy(a => a.SortIndex)
                                         .Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from advertisement in _context.Advertisements
                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
                                  join advtcamp in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advtcamp.AdvertisementId
                                  join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
                                  where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now && advertisement.AdvertisementId == advertisementIdDecrypted
                                  select new AdvertisementsForContentModel()
                                  {
                                      ContentId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString()
                                  }).AsEnumerable().GroupBy(x => x.ContentId).Select(a => a.First()).ToList().Count();
                }

                foreach (var content in contentsModelList)
                {
                    ContentsModel contentsModel = new ContentsModel()
                    {
                        ContentId = content.ContentId,
                        Type = content.Type,
                        Url = content.Url,
                        TintColor = content.TintColor,
                        InvertedTintColor = content.InvertedTintColor
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
                                    PromotionsModelForContent promotionsModelForContent = new PromotionsModelForContent();
                                    promotionsModelForContent.Title = promotion.Title;
                                    promotionsModelForContent.Subtitle = promotion.Subtitle;
                                    promotionsModelForContent.PromotionId = promotion.PromotionId;
                                    promotionsModelForContent.LogoUrl = promotion.LogoUrl;
                                    promotionsModelForContent.Code = promotion.Code;
                                    if (!string.IsNullOrEmpty(promotion.Type))
                                    {
                                        if (promotion.Type.ToLower() == "links")
                                        {
                                            promotionsModelForContent.Link = _appSettings.LinkUrlForContent + promotion.PromotionId;
                                        }
                                        else if (promotion.Type.ToLower() == "coupons")
                                        {
                                            promotionsModelForContent.Link = _appSettings.CouponUrlForContent + promotion.PromotionId;
                                        }
                                        else if (promotion.Type.ToLower() == "places")
                                        {
                                            promotionsModelForContent.Link = null;
                                        }
                                        else
                                        {
                                            promotionsModelForContent.Link = null;
                                        }
                                    }
                                    content.promotion = promotionsModelForContent;
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

                Intervals intervals = new Intervals();
                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    intervals = _context.Intervals.Where(x => x.IntervalId == ObfuscationClass.DecodeId(Convert.ToInt32(model.IntervalId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (intervals == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);
                }

                List<Campaigns> lstCampaign = new List<Campaigns>();
                foreach (var item in model.CampaignId)
                {
                    var campaign = _context.Campaigns.Where(x => x.CampaignId == ObfuscationClass.DecodeId(Convert.ToInt32(item), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (campaign == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                    lstCampaign.Add(campaign);
                }

                Advertisements advertisements = new Advertisements()
                {
                    CreatedAt = DateTime.UtcNow,
                    InstitutionId = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse),
                    MediaId = ObfuscationClass.DecodeId(Convert.ToInt32(model.MediaId), _appSettings.PrimeInverse),
                    ResourceName = model.ResourceName,
                    TintColor = model.TintColor,
                    InvertedTintColor = model.InvertedTintColor
                };
                _context.Advertisements.Add(advertisements);
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    AdvertisementsIntervals advertisementsintervals = new AdvertisementsIntervals()
                    {
                        AdvertisementId = advertisements.AdvertisementId,
                        IntervalId = intervals.IntervalId
                    };
                    _context.AdvertisementsIntervals.Add(advertisementsintervals);
                    _context.SaveChanges();
                }

                foreach (var item in lstCampaign)
                {
                    AdvertisementsCampaigns objAdvertisementscampaigns = new AdvertisementsCampaigns()
                    {
                        AdvertisementId = advertisements.AdvertisementId,
                        CampaignId = item.CampaignId
                    };
                    _context.AdvertisementsCampaigns.Add(objAdvertisementscampaigns);
                    _context.SaveChanges();
                }

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

                Intervals intervals = new Intervals();
                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    intervals = _context.Intervals.Where(x => x.IntervalId == ObfuscationClass.DecodeId(Convert.ToInt32(model.IntervalId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (intervals == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                }

                List<Campaigns> lstCampaign = new List<Campaigns>();
                foreach (var item in model.CampaignId)
                {
                    var campaign = _context.Campaigns.Where(x => x.CampaignId == ObfuscationClass.DecodeId(Convert.ToInt32(item), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (campaign == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                    lstCampaign.Add(campaign);
                }

                var advertisementsCampaign = _context.AdvertisementsCampaigns.Where(x => x.AdvertisementId == ObfuscationClass.DecodeId(Convert.ToInt32(model.AdvertisementId), _appSettings.PrimeInverse)).ToList();
                foreach (var item in advertisementsCampaign)
                {
                    _context.AdvertisementsCampaigns.Remove(item);
                    _context.SaveChanges();
                }

                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    var advertisementsinterval = advertisements.AdvertisementsIntervals.Where(x => x.AdvertisementId == ObfuscationClass.DecodeId(Convert.ToInt32(model.AdvertisementId), _appSettings.PrimeInverse)).FirstOrDefault();
                    if (advertisementsinterval == null)
                    {
                        AdvertisementsIntervals objAdvertisementsintervals = new AdvertisementsIntervals()
                        {
                            AdvertisementId = advertisements.AdvertisementId,
                            IntervalId = intervals.IntervalId
                        };
                        _context.AdvertisementsIntervals.Add(advertisementsinterval);
                        _context.SaveChanges();
                    }
                    else
                    {
                        advertisementsinterval.AdvertisementId = advertisements.AdvertisementId;
                        advertisementsinterval.IntervalId = intervals.IntervalId;
                        _context.AdvertisementsIntervals.Update(advertisementsinterval);
                        _context.SaveChanges();
                    }
                }

                foreach (var item in lstCampaign)
                {
                    AdvertisementsCampaigns objAdvertisementscampaigns = new AdvertisementsCampaigns()
                    {
                        AdvertisementId = advertisements.AdvertisementId,
                        CampaignId = item.CampaignId
                    };
                    _context.AdvertisementsCampaigns.Add(objAdvertisementscampaigns);
                    _context.SaveChanges();
                }

                advertisements.InstitutionId = ObfuscationClass.DecodeId(Convert.ToInt32(model.InstitutionId), _appSettings.PrimeInverse);
                advertisements.MediaId = ObfuscationClass.DecodeId(Convert.ToInt32(model.MediaId), _appSettings.PrimeInverse);
                advertisements.ResourceName = model.ResourceName;
                advertisements.TintColor = model.TintColor;
                advertisements.InvertedTintColor = model.InvertedTintColor;
                _context.Advertisements.Update(advertisements);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.AdvertisementUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateCampaignAdvertisement(string campaignsId, string advertisementsId, PatchSort model)
        {
            try
            {
                if (string.IsNullOrEmpty(campaignsId))
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignRequired, StatusCodes.Status400BadRequest);

                if (string.IsNullOrEmpty(advertisementsId))
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementRequired, StatusCodes.Status400BadRequest);

                int campaignsIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(campaignsId), _appSettings.PrimeInverse);
                int advertisementIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(advertisementsId), _appSettings.PrimeInverse);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                var advertisement = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted).FirstOrDefault();
                if (advertisement == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var camp_ads = _context.AdvertisementsCampaigns.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                if (camp_ads == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementCampaignNotFound, StatusCodes.Status404NotFound);

                camp_ads.SortIndex = model.Sort;
                _context.AdvertisementsCampaigns.Update(camp_ads);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.SortUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateCampaignAdvertisementList(string campaignsId, PatchSortList model)
        {
            try
            {
                if (string.IsNullOrEmpty(campaignsId))
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignRequired, StatusCodes.Status400BadRequest);

                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.EmptyModel, StatusCodes.Status400BadRequest);

                int campaignsIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(campaignsId), _appSettings.PrimeInverse);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                foreach (var item in model.SortItem)
                {
                    if (string.IsNullOrEmpty(item.AdvertisementId))
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignRequired, StatusCodes.Status400BadRequest);

                    int advertisementIdDecrypted = ObfuscationClass.DecodeId(Convert.ToInt32(item.AdvertisementId), _appSettings.PrimeInverse);

                    var advertisement = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted).FirstOrDefault();
                    if (advertisement == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                    var camp_ads = _context.AdvertisementsCampaigns.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                    if (camp_ads == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementCampaignNotFound, StatusCodes.Status404NotFound);

                    camp_ads.SortIndex = item.Sort;
                    _context.AdvertisementsCampaigns.Update(camp_ads);
                    _context.SaveChanges();
                }
                return ReturnResponse.SuccessResponse(CommonMessage.SortUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}