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
using RoutesSecurity;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Threading.Tasks;
using RestSharp;
using System.Net;
using System.IO;
using Microsoft.Extensions.Logging;

namespace AdvertisementService.Repository
{
    public class AdvertisementsRepository : IAdvertisementsRepository
    {
        private readonly AdvertisementContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        private readonly AppSettings _appSettings;
        private IWebHostEnvironment _hostingEnv;
        private ICommonFunctions _commonFunctions;
        private readonly AzureStorageBlobConfig _config;
        private readonly Dependencies _dependencies;
        private readonly IVideoConversionRepository _videoConversionRepository;
        private readonly ILogger<AdvertisementsRepository> _logger;
        public AdvertisementsRepository(IOptions<AppSettings> appSettings, AdvertisementContext context, IIncludeAdvertisementsRepository includeAdvertisements, IWebHostEnvironment hostingEnv, ICommonFunctions commonFunctions, IOptions<AzureStorageBlobConfig> config, IOptions<Dependencies> dependencies, IVideoConversionRepository videoConversionRepository, ILogger<AdvertisementsRepository> logger)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
            _hostingEnv = hostingEnv;
            _commonFunctions = commonFunctions;
            _config = config.Value;
            _dependencies = dependencies.Value;
            _videoConversionRepository = videoConversionRepository;
            _logger = logger;
        }

        public async Task<dynamic> DeleteAdvertisementsAsync(string id)
        {
            try
            {
                int advertisementIdDecrypted = Obfuscation.Decode(id);
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.Broadcasts).Include(x => x.Media).Where(x => x.AdvertisementId == advertisementIdDecrypted).FirstOrDefault();
                if (advertisements == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                if (advertisements.AdvertisementsIntervals != null)
                    _context.AdvertisementsIntervals.RemoveRange(advertisements.AdvertisementsIntervals);

                if (advertisements.Broadcasts != null)
                    _context.Broadcasts.RemoveRange(advertisements.Broadcasts);

                if (advertisements.Media != null)
                {
                    var mediaReferenceName = advertisements.Media.Url.Split('/');
                    if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                    {
                        CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = BlobClient.GetContainerReference(_config.Container);
                        if (await container.ExistsAsync())
                        {
                            CloudBlob file = container.GetBlobReference(mediaReferenceName.LastOrDefault());
                            if (await file.ExistsAsync())
                                await file.DeleteAsync();
                        }
                    }
                    _context.Medias.Remove(advertisements.Media);
                }

                var promotionIds = GetPromotionsIds(id);
                var linkIds = GetLinksIds(promotionIds);
                if (linkIds != null && linkIds.Count > 0)
                {
                    foreach (var item in linkIds)
                    {
                        DeleteLinks(item);
                    }
                }

                var couponIds = GetCouponsIds(promotionIds);
                if (couponIds != null && couponIds.Count > 0)
                {
                    foreach (var item in couponIds)
                    {
                        DeleteCoupons(item);
                    }
                }

                if (promotionIds != null && promotionIds.Count > 0)
                {
                    foreach (var item in promotionIds)
                    {
                        DeletePromotions(item);
                    }
                }

                _context.Advertisements.Remove(advertisements);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.AdvertisementDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetAdvertisements(string institutionId, string advertisementId, string includeType, string embed, string sort_by, Pagination pageInfo)
        {

            int totalCount = 0;
            try
            {
                AdvertisementsGetResponse response = new AdvertisementsGetResponse();
                List<AdvertisementsGetModel> advertisementsModelList = new List<AdvertisementsGetModel>();
                List<string> campainIds = new List<string>();

                if (string.IsNullOrEmpty(institutionId))
                {
                    if (string.IsNullOrEmpty(advertisementId))
                    {
                        var advertisements = _context.Advertisements.Include(x => x.Broadcasts).ToList();
                        var BroadcastsData = _context.AdvertisementsIntervals.ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, BroadcastsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                    else
                    {
                        int advertisementIdDecrypted = Obfuscation.Decode(advertisementId);
                        var advertisements = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted).ToList();
                        var BroadcastsData = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == advertisementIdDecrypted).ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, BroadcastsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(advertisementId))
                    {
                        int institutionIdDecrypted = Obfuscation.Decode(institutionId);
                        var advertisements = _context.Advertisements.Where(x => x.InstitutionId == institutionIdDecrypted).ToList();
                        var BroadcastsData = _context.AdvertisementsIntervals.Where(x => x.Advertisement.InstitutionId == institutionIdDecrypted).ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, BroadcastsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                    else
                    {
                        int institutionIdDecrypted = Obfuscation.Decode(institutionId);
                        int advertisementIdDecrypted = Obfuscation.Decode(advertisementId);
                        var advertisements = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.InstitutionId == institutionIdDecrypted).ToList();
                        var BroadcastsData = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.Advertisement.InstitutionId == institutionIdDecrypted).ToList();
                        var advertisementsModelListWithCampaign = _commonFunctions.GetAllAdvertisements(advertisements, BroadcastsData, pageInfo);
                        advertisementsModelList = _commonFunctions.GetAdvertisementWithCampaigns(advertisementsModelListWithCampaign);
                        totalCount = advertisements.Count();
                    }
                }

                if (!string.IsNullOrEmpty(embed) && embed.ToLower() == "sort")
                {
                    foreach (var item in advertisementsModelList)
                    {
                        int adsId = Obfuscation.Decode(item.AdvertisementId);
                        if (!string.IsNullOrEmpty(item.CampaignId.FirstOrDefault()))
                        {
                            int campId = Obfuscation.Decode(item.CampaignId.FirstOrDefault());
                            item.Sort = _context.Broadcasts.Where(x => x.AdvertisementId == adsId && x.CampaignId == campId).Select(x => x.Sort).FirstOrDefault();
                        }
                        else
                            item.Sort = _context.Broadcasts.Where(x => x.AdvertisementId == adsId).Select(x => x.Sort).FirstOrDefault();
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


        public dynamic GetContents(string advertisementId, Pagination pageInfo)
        {
            return JObject.Parse(@"
            {
                ""pagination"": {
                    ""offset"": 1,
                    ""limit"": 10,
                    ""total"": 10
                },
                ""data"": [
                    {
                        ""contentId"": ""A879983343"",
                        ""type"": ""image"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/bdafcbcf-3c01-4867-9b30-162988881683.png"",
                        ""resourceNumber"": ""A0010"",
                        ""name"": ""Routes Insta Banner"",
                        ""tintColor"": 2839179
                    },
                    {
                        ""contentId"": ""A312529868"",
                        ""type"": ""image"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/43fe0241-0292-4aa4-b2fc-0646e02b6ea9.png"",
                        ""resourceNumber"": ""A0011"",
                        ""name"": ""Routes FB Banner"",
                        ""tintColor"": 2839179
                    },
                    {
                        ""contentId"": ""A356727653"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/31ff985c-9352-436c-a4cb-1e560a5effa7.mp4"",
                        ""resourceNumber"": ""A0024"",
                        ""name"": ""Vaccination is protection"",
                        ""promotion"": {
                            ""promotionId"": ""A356727653"",
                            ""title"": ""VACCINATION IS PROTECTION   -   ‏التــطعيم وقــايــة"",
                            ""subtitle"": ""Visit the website or scan the code  -  زورو الموقع"",
                            ""link"": ""http://links.routesme.com/A356727653""
                        }
                    },
                    {
                        ""contentId"": ""A390662335"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/860f0d6d-2cfc-48e0-9c76-bfe3975436ae.mp4"",
                        ""resourceNumber"": ""A0026"",
                        ""name"": ""Routes_Covid"",
                        ""tintColor"": 41974
                    },
                    {
                        ""contentId"": ""A1970692508"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/5072b331-8a30-462e-9db3-aef636273d42.mp4"",
                        ""resourceNumber"": ""A0027"",
                        ""name"": ""Nescafe LQ"",
                        ""tintColor"": 5221192
                    },
                    {
                        ""contentId"": ""A903654922"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/a12e3534-ac87-4d05-b301-686d878c484a.mp4"",
                        ""resourceNumber"": ""A0012"",
                        ""name"": ""CityCenter"",
                        ""tintColor"": 248529,
                        ""promotion"": {
                            ""promotionId"": ""A1814427574"",
                            ""title"": ""Download City Centre App today!"",
                            ""subtitle"":""حمل تطبيق ستي سنتر الآن"",
                            ""link"": ""http://stage.links.routesme.com/A1814427574""
                        }
                    },
                    {
                        ""contentId"": ""A2093022760"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/a0d673aa-3d8b-48ac-afb8-4b0876d97d77.mp4"",
                        ""resourceNumber"": ""A0023"",
                        ""name"": ""New Year 2022"",
                        ""tintColor"": 1655439
                    },
                    {
                        ""contentId"": ""A1892560041"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/71597500-9524-44bb-ba44-3f6ec364b9af.mp4"",
                        ""resourceNumber"": ""A0012"",
                        ""name"": ""McWrap"",
                        ""tintColor"": 16497182,
                        ""promotion"": {
                            ""promotionId"": ""A434860120"",
                            ""title"": ""Dowload McDonald’s app today"",
                            ""link"": ""http://links.routesme.com/A434860120""
                        }
                    },
                    {
                        ""contentId"": ""A1046511380"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/bea7a5bd-b70c-453e-8e22-1487aca833ba.mp4"",
                        ""resourceNumber"": ""A0018"",
                        ""name"": ""QatarAirways"",
                        ""tintColor"": 14091268,
                        ""promotion"": {
                            ""promotionId"": ""A1369304351"",
                            ""title"": ""Economy Class experience like never before"",
                            ""subtitle"":""Book your journey now"",
                            ""link"": ""http://stage.links.routesme.com/A1369304351""
                        }
                    },
                    {
                        ""contentId"": ""A1715768901"",
                        ""type"": ""image"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/55652e10-a9db-45bf-baa6-8d889269cbc9.jpg"",
                        ""resourceNumber"": ""A0012"",
                        ""name"": ""0.0000000000"",
                        ""tintColor"": 2711234,
                        ""promotion"": {
                            ""promotionId"": ""A1936757826"",
                            ""title"": ""123"",
                            ""subtitle"": ""test to test"",
                            ""link"": ""http://stage.links.routesme.com/A1936757826""
                        }
                    }
                ],
                ""status"": true,
                ""message"": ""Contents retrived successfully."",
                ""statusCode"": 200
            }");
        }

        // public dynamic GetContents(string advertisementId, Pagination pageInfo)
        // {
        //     int totalCount = 0;
        //     try
        //     {
        //         ContentsGetResponse response = new ContentsGetResponse();
        //         List<AdvertisementsForContentModel> contentsModelList = new List<AdvertisementsForContentModel>();
        //         MediasModel medias = new MediasModel();
        //         List<ContentsModel> contents = new List<ContentsModel>();

        //         if (string.IsNullOrEmpty(advertisementId))
        //         {
        //             contentsModelList = (from advertisement in _context.Advertisements
        //                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
        //                                  join advtcamp in _context.Broadcasts on advertisement.AdvertisementId equals advtcamp.AdvertisementId
        //                                  join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
        //                                  where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now
        //                                  select new AdvertisementsForContentModel()
        //                                  {
        //                                      ContentId = Obfuscation.Encode(advertisement.AdvertisementId),
        //                                      Type = media.MediaType,
        //                                      Url = media.Url,
        //                                      Sort = advtcamp.Sort,
        //                                      ResourceNumber = advertisement.ResourceNumber,
        //                                      Name = advertisement.Name,
        //                                      TintColor = advertisement.TintColor,
        //                                      InvertedTintColor = advertisement.InvertedTintColor
        //                                  }).AsEnumerable().OrderBy(a => a.Sort)
        //                                  .Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

        //             totalCount =
        //             (from advertisement in _context.Advertisements
        //              join media in _context.Medias on advertisement.MediaId equals media.MediaId
        //              join advtcamp in _context.Broadcasts on advertisement.AdvertisementId equals advtcamp.AdvertisementId
        //              join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
        //              where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now
        //              select new AdvertisementsForContentModel()
        //              {
        //                  ContentId = Obfuscation.Encode(advertisement.AdvertisementId)
        //              }).AsEnumerable().ToList().Count();
        //         }
        //         else
        //         {
        //             int advertisementIdDecrypted = Obfuscation.Decode(advertisementId);
        //             contentsModelList = (from advertisement in _context.Advertisements
        //                                  join media in _context.Medias on advertisement.MediaId equals media.MediaId
        //                                  join advtcamp in _context.Broadcasts on advertisement.AdvertisementId equals advtcamp.AdvertisementId
        //                                  join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
        //                                  where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now && advertisement.AdvertisementId == advertisementIdDecrypted
        //                                  select new AdvertisementsForContentModel()
        //                                  {
        //                                      ContentId = Obfuscation.Encode(advertisement.AdvertisementId),
        //                                      Type = media.MediaType,
        //                                      Url = media.Url,
        //                                      Sort = advtcamp.Sort,
        //                                      ResourceNumber = advertisement.ResourceNumber,
        //                                      Name = advertisement.Name,
        //                                      TintColor = advertisement.TintColor,
        //                                      InvertedTintColor = advertisement.InvertedTintColor
        //                                  }).AsEnumerable().GroupBy(x => x.ContentId).Select(a => a.First()).OrderBy(a => a.Sort)
        //                                  .Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

        //             totalCount = (from advertisement in _context.Advertisements
        //                           join media in _context.Medias on advertisement.MediaId equals media.MediaId
        //                           join advtcamp in _context.Broadcasts on advertisement.AdvertisementId equals advtcamp.AdvertisementId
        //                           join camp in _context.Campaigns on advtcamp.CampaignId equals camp.CampaignId
        //                           where camp.Status.ToLower() == "active" && camp.StartAt <= DateTime.Now && camp.EndAt >= DateTime.Now && advertisement.AdvertisementId == advertisementIdDecrypted
        //                           select new AdvertisementsForContentModel()
        //                           {
        //                               ContentId = Obfuscation.Encode(advertisement.AdvertisementId)
        //                           }).AsEnumerable().GroupBy(x => x.ContentId).Select(a => a.First()).ToList().Count();
        //         }

        //         foreach (var content in contentsModelList)
        //         {
        //             ContentsModel contentsModel = new ContentsModel()
        //             {
        //                 ContentId = content.ContentId,
        //                 Type = content.Type,
        //                 Url = content.Url,
        //                 Name = content.Name,
        //                 ResourceNumber = content.ResourceNumber,
        //                 TintColor = content.TintColor,
        //                 InvertedTintColor = content.InvertedTintColor
        //             };
        //             contents.Add(contentsModel);
        //         }

        //         if (contentsModelList.Count > 0)
        //         {
        //             List<PromotionsGetModel> promotions = _includeAdvertisements.GetPromotionsIncludedData(contentsModelList);
        //             if (promotions != null && promotions.Count > 0)
        //             {
        //                 foreach (var content in contents)
        //                 {
        //                     foreach (var promotion in promotions)
        //                     {
        //                         if (content.ContentId == promotion.AdvertisementId)
        //                         {
        //                             PromotionsModelForContent promotionsModelForContent = new PromotionsModelForContent();
        //                             promotionsModelForContent.Title = promotion.Title;
        //                             promotionsModelForContent.Subtitle = promotion.Subtitle;
        //                             promotionsModelForContent.PromotionId = promotion.PromotionId;
        //                             promotionsModelForContent.LogoUrl = promotion.LogoUrl;
        //                             promotionsModelForContent.Code = promotion.Code;
        //                             if (!string.IsNullOrEmpty(promotion.Type))
        //                             {
        //                                 if (promotion.Type.ToLower() == "links")
        //                                 {
        //                                     promotionsModelForContent.Link = _appSettings.LinkUrlForContent + promotion.PromotionId;
        //                                 }
        //                                 else if (promotion.Type.ToLower() == "coupons")
        //                                 {
        //                                     promotionsModelForContent.Link = _appSettings.CouponUrlForContent + promotion.PromotionId;
        //                                 }
        //                                 else if (promotion.Type.ToLower() == "places")
        //                                 {
        //                                     promotionsModelForContent.Link = null;
        //                                 }
        //                                 else
        //                                 {
        //                                     promotionsModelForContent.Link = null;
        //                                 }
        //                             }
        //                             content.promotion = promotionsModelForContent;
        //                         }
        //                     }
        //                 }
        //             }
        //         }

        //         var page = new Pagination
        //         {
        //             offset = pageInfo.offset,
        //             limit = pageInfo.limit,
        //             total = totalCount
        //         };

        //         response.status = true;
        //         response.message = CommonMessage.ContentsRetrive;
        //         response.pagination = page;
        //         response.data = contents;
        //         response.statusCode = StatusCodes.Status200OK;
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         return ReturnResponse.ExceptionResponse(ex);
        //     }
        // }

        public async Task<dynamic> InsertAdvertisementsAsync(PostAdvertisementsModel model)
        {
            AdvertisementsPostResponse response = new AdvertisementsPostResponse();
            try
            {
                string mediaReferenceName = string.Empty, ext = string.Empty;
                int? mediaId = null, MediaMetadataId = null;
                Intervals intervals = new Intervals();
                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    intervals = _context.Intervals.Where(x => x.IntervalId == Obfuscation.Decode(model.IntervalId)).FirstOrDefault();
                    if (intervals == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);
                }

                List<Campaigns> lstCampaign = new List<Campaigns>();
                foreach (var item in model.CampaignId)
                {
                    var campaign = _context.Campaigns.Where(x => x.CampaignId == Obfuscation.Decode(item)).FirstOrDefault();
                    if (campaign == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                    lstCampaign.Add(campaign);
                }

                if (!string.IsNullOrEmpty(model.MediaUrl))
                {
                    var existingMediaReferenceName = model.MediaUrl.Split('/');
                    ext = existingMediaReferenceName.Last().Split('.').Last();
                    VideoMetadata videoMetadata = new VideoMetadata();
                    if (ext == "mp4")
                    {
                        if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                        {
                            // videoMetadata = await _videoConversionRepository.ConvertVideoAsync(model.MediaUrl);
                            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                            CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                            if (!string.IsNullOrEmpty(videoMetadata.CompressedFile))
                            {
                                if (await container.ExistsAsync())
                                {
                                    CloudBlob file = container.GetBlobReference(existingMediaReferenceName.LastOrDefault());
                                    if (await file.ExistsAsync())
                                        await file.DeleteAsync();
                                }
                                mediaReferenceName = videoMetadata.CompressedFile.Split("\\").LastOrDefault();
                                CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                                using (var stream = File.OpenRead(videoMetadata.CompressedFile))
                                {
                                    await blockBlob.UploadFromStreamAsync(stream);
                                }
                                model.MediaUrl = blockBlob.Uri.AbsoluteUri;
                                _logger.LogInformation(blockBlob.Uri.AbsoluteUri);
                                FileInfo fInfo = new FileInfo(videoMetadata.CompressedFile);
                                fInfo.Delete();
                            }
                        }

                        MediaMetadata mediaMetadata = new MediaMetadata();
                        mediaMetadata.Duration = videoMetadata.Duration;
                        mediaMetadata.Size = videoMetadata.VideoSize;
                        _context.MediaMetadata.Add(mediaMetadata);
                        _context.SaveChanges();
                        MediaMetadataId = mediaMetadata.MediaMetadataId;
                    }
                    else if (ext == "jpg" || ext == "png" || ext == "jpeg")
                    {
                        var imagesize = await _videoConversionRepository.ConvertImageAsync(model.MediaUrl);
                        MediaMetadata mediaMetadata = new MediaMetadata();
                        mediaMetadata.Duration = 0;
                        mediaMetadata.Size = imagesize;
                        _context.MediaMetadata.Add(mediaMetadata);
                        _context.SaveChanges();
                        MediaMetadataId = mediaMetadata.MediaMetadataId;
                    }

                    Medias media = new Medias();
                    media.Url = model.MediaUrl;
                    media.CreatedAt = DateTime.Now;
                    if (ext == "mp4")
                        media.MediaType = "video";
                    else if (ext == "jpg" || ext == "png" || ext == "jpeg")
                        media.MediaType = "image";
                    media.MediaMetadataId = MediaMetadataId;
                    _context.Medias.Add(media);
                    _context.SaveChanges();
                    mediaId = media.MediaId;
                }

                Advertisements advertisements = new Advertisements()
                {
                    CreatedAt = DateTime.UtcNow,
                    InstitutionId = Obfuscation.Decode(model.InstitutionId),
                    MediaId = mediaId,
                    ResourceNumber = JsonConvert.DeserializeObject<ResourceNamesResponse>(GetAPI(_dependencies.IdentifiersUrl, "key=advertisements").Content).resourceName.ToString(),
                    Name = model.Name,
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
                    Broadcasts objBroadcasts = new Broadcasts()
                    {
                        AdvertisementId = advertisements.AdvertisementId,
                        CampaignId = item.CampaignId,
                        CreatedAt = DateTime.Now
                    };
                    _context.Broadcasts.Add(objBroadcasts);
                    _context.SaveChanges();
                }

                response.status = true;
                response.statusCode = StatusCodes.Status201Created;
                response.message = CommonMessage.AdvertisementInsert;
                response.AdvertisementId = Obfuscation.Encode(advertisements.AdvertisementId);
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> UpdateAdvertisementsAsync(PostAdvertisementsModel model)
        {
            try
            {
                string mediaReferenceName = string.Empty, ext = string.Empty;
                int? MediaMetadataId = null;
                var advertisements = _context.Advertisements.Include(x => x.AdvertisementsIntervals).Include(x => x.Broadcasts).Where(x => x.AdvertisementId == Obfuscation.Decode(model.AdvertisementId)).FirstOrDefault();
                if (advertisements == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                Intervals intervals = new Intervals();
                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    intervals = _context.Intervals.Where(x => x.IntervalId == Obfuscation.Decode(model.IntervalId)).FirstOrDefault();
                    if (intervals == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);
                }
                Medias mediaData = new Medias();
                if (model.MediaUrl != null)
                {
                    mediaData = _context.Medias.Include(x => x.MediaMetadata).Where(x => x.Url == model.MediaUrl).FirstOrDefault();
                    if (mediaData == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);
                }

                List<Campaigns> lstCampaign = new List<Campaigns>();
                foreach (var item in model.CampaignId)
                {
                    var campaign = _context.Campaigns.Where(x => x.CampaignId == Obfuscation.Decode(item)).FirstOrDefault();
                    if (campaign == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                    lstCampaign.Add(campaign);
                }

                var advertisementsCampaign = _context.Broadcasts.Where(x => x.AdvertisementId == Obfuscation.Decode(model.AdvertisementId)).ToList();
                foreach (var item in advertisementsCampaign)
                {
                    _context.Broadcasts.Remove(item);
                    _context.SaveChanges();
                }

                if (!string.IsNullOrEmpty(model.IntervalId))
                {
                    var advertisementsinterval = advertisements.AdvertisementsIntervals.Where(x => x.AdvertisementId == Obfuscation.Decode(model.AdvertisementId)).FirstOrDefault();
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
                    Broadcasts objBroadcasts = new Broadcasts()
                    {
                        AdvertisementId = advertisements.AdvertisementId,
                        CampaignId = item.CampaignId
                    };
                    _context.Broadcasts.Add(objBroadcasts);
                    _context.SaveChanges();
                }

                if (!string.IsNullOrEmpty(model.MediaUrl))
                {
                    var existingMediaReferenceName = model.MediaUrl.Split('/');
                    ext = existingMediaReferenceName.Last().Split('.').Last();
                    VideoMetadata videoMetadata = new VideoMetadata();
                    if (ext == "mp4")
                    {
                        if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                        {
                            // videoMetadata = await _videoConversionRepository.ConvertVideoAsync(model.MediaUrl);
                            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                            CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                            if (await container.ExistsAsync())
                            {
                                CloudBlob file = container.GetBlobReference(existingMediaReferenceName.LastOrDefault());
                                if (await file.ExistsAsync())
                                    await file.DeleteAsync();
                            }
                            mediaReferenceName = videoMetadata.CompressedFile.Split("\\").LastOrDefault();
                            CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                            using (var stream = File.OpenRead(videoMetadata.CompressedFile))
                            {
                                await blockBlob.UploadFromStreamAsync(stream);
                            }
                            model.MediaUrl = blockBlob.Uri.AbsoluteUri;
                            FileInfo fInfo = new FileInfo(videoMetadata.CompressedFile);
                            fInfo.Delete();
                        }
                    }

                    if (mediaData.MediaMetadata == null)
                    {
                        MediaMetadata metadataItem = new MediaMetadata();
                        if (ext == "mp4")
                        {
                            metadataItem.Duration = videoMetadata.Duration;
                            metadataItem.Size = videoMetadata.VideoSize;
                        }
                        else if (ext == "jpg" || ext == "png" || ext == "jpeg")
                        {
                            var imagesize = await _videoConversionRepository.ConvertImageAsync(model.MediaUrl);
                            metadataItem.Duration = 0;
                            metadataItem.Size = imagesize;
                        }
                        _context.MediaMetadata.Add(metadataItem);
                        _context.SaveChanges();
                        MediaMetadataId = metadataItem.MediaMetadataId;
                    }
                    else
                    {
                        if (ext == "mp4")
                        {
                            mediaData.MediaMetadata.Duration = videoMetadata.Duration;
                            mediaData.MediaMetadata.Size = videoMetadata.VideoSize;
                        }
                        else if (ext == "jpg" || ext == "png" || ext == "jpeg")
                        {
                            var imagesize = await _videoConversionRepository.ConvertImageAsync(model.MediaUrl);
                            mediaData.MediaMetadata.Duration = 0;
                            mediaData.MediaMetadata.Size = imagesize;
                        }
                        _context.MediaMetadata.Update(mediaData.MediaMetadata);
                        _context.SaveChanges();

                    }
                    mediaData.Url = model.MediaUrl;
                    if (ext == "mp4")
                        mediaData.MediaType = "video";
                    else if (ext == "jpg" || ext == "png" || ext == "jpeg")
                        mediaData.MediaType = "image";
                    _context.Medias.Update(mediaData);
                    _context.SaveChanges();
                }

                advertisements.InstitutionId = Obfuscation.Decode(model.InstitutionId);
                if (mediaData != null)
                    advertisements.MediaId = mediaData.MediaId;
                else
                    advertisements.MediaId = null;
                advertisements.ResourceNumber = model.ResourceNumber;
                advertisements.Name = model.Name;
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

                int campaignsIdDecrypted = Obfuscation.Decode(campaignsId);
                int advertisementIdDecrypted = Obfuscation.Decode(advertisementsId);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                var advertisement = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted).FirstOrDefault();
                if (advertisement == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                var camp_ads = _context.Broadcasts.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                if (camp_ads == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.BroadcastNotFound, StatusCodes.Status404NotFound);

                camp_ads.Sort = model.Sort;
                _context.Broadcasts.Update(camp_ads);
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
                    return ReturnResponse.ErrorResponse(CommonMessage.InvalidData, StatusCodes.Status400BadRequest);

                int campaignsIdDecrypted = Obfuscation.Decode(campaignsId);

                var campaign = _context.Campaigns.Where(x => x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                foreach (var item in model.SortItem)
                {
                    if (string.IsNullOrEmpty(item.AdvertisementId))
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignRequired, StatusCodes.Status400BadRequest);

                    int advertisementIdDecrypted = Obfuscation.Decode(item.AdvertisementId);

                    var advertisement = _context.Advertisements.Where(x => x.AdvertisementId == advertisementIdDecrypted).FirstOrDefault();
                    if (advertisement == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, StatusCodes.Status404NotFound);

                    var camp_ads = _context.Broadcasts.Where(x => x.AdvertisementId == advertisementIdDecrypted && x.CampaignId == campaignsIdDecrypted).FirstOrDefault();
                    if (camp_ads == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.BroadcastNotFound, StatusCodes.Status404NotFound);

                    camp_ads.Sort = item.Sort;
                    _context.Broadcasts.Update(camp_ads);
                    _context.SaveChanges();
                }
                return ReturnResponse.SuccessResponse(CommonMessage.SortUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        private dynamic GetAPI(string url, string query = "")
        {
            UriBuilder uriBuilder = new UriBuilder(_appSettings.Host + url);
            uriBuilder = AppendQueryToUrl(uriBuilder, query);
            var client = new RestClient(uriBuilder.Uri);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == 0)
                throw new HttpListenerException(400, CommonMessage.ConnectionFailure);

            if (!response.IsSuccessful)
                throw new HttpListenerException((int)response.StatusCode, response.Content);

            return response;
        }

        private UriBuilder AppendQueryToUrl(UriBuilder baseUri, string queryToAppend)
        {
            if (baseUri.Query != null && baseUri.Query.Length > 1)
                baseUri.Query = baseUri.Query.Substring(1) + "&" + queryToAppend;
            else
                baseUri.Query = queryToAppend;
            return baseUri;
        }

        private void DeletePromotions(string promotionId)
        {
            try
            {
                var client = new RestClient(_appSettings.Host + _dependencies.PromotionsUrl + promotionId);
                var request = new RestRequest(Method.DELETE);
                IRestResponse response = client.Execute(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteCoupons(string couponId)
        {
            try
            {
                var client = new RestClient(_appSettings.Host + _dependencies.DeleteCouponsUrl + couponId);
                var request = new RestRequest(Method.DELETE);
                IRestResponse response = client.Execute(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteLinks(string linkId)
        {
            try
            {
                var client = new RestClient(_appSettings.Host + _dependencies.DeleteLinksUrl + linkId);
                var request = new RestRequest(Method.DELETE);
                IRestResponse response = client.Execute(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> GetPromotionsIds(string advertisementId)
        {
            try
            {
                List<string> promotionIds = new List<string>();
                var client = new RestClient(_appSettings.Host + _dependencies.PromotionsByAdvertisementUrl + advertisementId);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var promotionData = JsonConvert.DeserializeObject<PromotionsGetResponse>(result);
                    foreach (var item in promotionData.data)
                    {
                        promotionIds.Add(item.PromotionId);
                    }
                }
                return promotionIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> GetCouponsIds(List<string> promotionsIds)
        {
            try
            {
                List<string> couponsIds = new List<string>();
                foreach (var item in promotionsIds)
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.GetCouponsUrl + item);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var coupons = JsonConvert.DeserializeObject<CouponResponse>(result);
                        foreach (var innerItem in coupons.data)
                        {
                            couponsIds.Add(innerItem.CouponId);
                        }
                    }
                }
                return couponsIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> GetLinksIds(List<string> promotionsIds)
        {
            try
            {
                List<string> linksIds = new List<string>();
                foreach (var item in promotionsIds)
                {
                    var client = new RestClient(_appSettings.Host + _dependencies.GetLinksUrl + item);
                    var request = new RestRequest(Method.GET);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = response.Content;
                        var links = JsonConvert.DeserializeObject<LinkResponse>(result);
                        foreach (var innerItem in links.data)
                        {
                            linksIds.Add(innerItem.LinkId);
                        }
                    }
                }
                return linksIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}