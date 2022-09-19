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


        public dynamic GetContents(string advertisementId, Pagination pageInfo,string institutionId)
        {

            if(!string.IsNullOrEmpty(institutionId))
            {
                int advertisementIdDecrypted = Obfuscation.Decode(institutionId);
                if(advertisementIdDecrypted==32)
                {
                    return JObject.Parse(@"{
                    ""pagination"": {
                        ""offset"": 1,
                        ""limit"": 15,
                        ""total"": 11
                    },
                    ""data"": [
                        {
                            ""contentId"": ""A591125054"",
                            ""type"": ""image"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/420e155f-6dee-471f-a2c6-e8a96b6e9625.jpg"",
                            ""resourceNumber"": ""A0037"",
                            ""name"": ""Vergin Banner"",
                            ""tintColor"": 12386560
                        },
                        {
                            ""contentId"": ""A1603701752"",
                            ""type"": ""image"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/VerginBEng.jpg"",
                            ""resourceNumber"": ""A0039"",
                            ""name"": ""Virgin Banner Eng"",
                            ""tintColor"": 16121857
                        },
                        {
                            ""contentId"": ""A1036248277"",
                            ""type"": ""image"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/VerginBAr.jpg"",
                            ""resourceNumber"": ""A0040"",
                            ""name"": ""Virgin Banner Arb"",
                            ""tintColor"": 16121857
                        },
                        {
                            ""contentId"": ""A1280908781"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/e4204320-ce54-4b49-9000-9a1de24fb0ab.mp4"",
                            ""resourceNumber"": ""A0032"",
                            ""name"": ""AirportVideoAmerni"",
                            ""tintColor"": 1589903
                        },
                        {
                            ""contentId"": ""A23671579"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/Atyab250822.mp4"",
                            ""resourceNumber"": ""A0038"",
                            ""name"": ""Atyab"",
                            ""tintColor"": 921102
                        },
                        {
                            ""contentId"": ""A468794802"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/FreeWifi_Eng.mp4"",
                            ""resourceNumber"": ""A0041"",
                            ""name"": ""Virgin Video Wifi"",
                            ""tintColor"": 15271942
                        },
                        {
                            ""contentId"": ""A1481371500"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/dbcdb875-311c-406e-ac20-cd77833f0cb4.mp4"",
                            ""resourceNumber"": ""A0043"",
                            ""name"": ""Virgin Video Wifi Ar"",
                            ""tintColor"": 12388360
                        },
                        {
                            ""contentId"": ""A23671579"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/Atyab250822.mp4"",
                            ""resourceNumber"": ""A0038"",
                            ""name"": ""Atyab"",
                            ""tintColor"": 921102
                        },
                        {
                            ""contentId"": ""A913918025"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/d078c23a-7be3-4c59-b886-395b5ad5c443.mp4"",
                            ""resourceNumber"": ""A0044"",
                            ""name"": ""Virgin Video NewSim Ar"",
                            ""tintColor"": 15009801
                        },
                        {
                            ""contentId"": ""A23671579"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/Atyab250822.mp4"",
                            ""resourceNumber"": ""A0038"",
                            ""name"": ""Atyab"",
                            ""tintColor"": 921102
                        },
                        {
                            ""contentId"": ""A2048824975"",
                            ""type"": ""video"",
                            ""url"": ""https://routesme.blob.core.windows.net/advertisements/Virgin%20Video%20NewSim%20Eng.mp4"",
                            ""resourceNumber"": ""A0042"",
                            ""name"": ""Virgin Video NewSim"",
                            ""tintColor"": 15468295
                        }
                    ],
                    ""status"": true,
                    ""message"": ""Contents retrived successfully."",
                    ""statusCode"": 200
                    }");
                }
            }
            
            
            return JObject.Parse(@"
            {
                ""pagination"": {
                    ""offset"": 1,
                    ""limit"": 10,
                    ""total"": 10
                },
                ""data"": [
                    {
                        ""contentId"": ""A591125054"",
                        ""type"": ""image"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/420e155f-6dee-471f-a2c6-e8a96b6e9625.jpg"",
                        ""resourceNumber"": ""A0037"",
                        ""name"": ""Vergin Banner"",
                        ""tintColor"": 12386560
                    },
                    {
                        ""contentId"": ""A1603701752"",
                        ""type"": ""image"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/9e5566a9-6ef0-424d-afb5-834b114ac692.jpg"",
                        ""resourceNumber"": ""A0039"",
                        ""name"": ""Virgin Banner Eng"",
                        ""tintColor"": 16121857
                    },
                    {
                        ""contentId"": ""A1036248277"",
                        ""type"": ""image"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/d207ca4d-09fb-45b4-8051-a6eef042a8b7.jpg"",
                        ""resourceNumber"": ""A0040"",
                        ""name"": ""Virgin Banner Arb"",
                        ""tintColor"": 16121857
                    },
                    {
                        ""contentId"": ""A468794802"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/FreeWifi_Eng.mp4"",
                        ""resourceNumber"": ""A0041"",
                        ""name"": ""Virgin Video Wifi"",
                        ""tintColor"": 15271942
                    },
                    {
                        ""contentId"": ""A1481371500"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/dbcdb875-311c-406e-ac20-cd77833f0cb4.mp4"",
                        ""resourceNumber"": ""A0043"",
                        ""name"": ""Virgin Video Wifi Ar"",
                        ""tintColor"": 12388360
                    },
                    {
                        ""contentId"": ""A1848362256"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/e235260f-a23b-4cca-90d2-1a73b43d0adf.mp4"",
                        ""resourceNumber"": ""A0031"",
                        ""name"": ""FlyBooking"",
                        ""tintColor"": 15693114,
                        ""promotion"": {
                            ""promotionId"": ""A312529868"",
                            ""title"": ""Download Flybooking and get the best rates for tickets and hotels."",
                            ""subtitle"": ""حمل تطبيق فلاي بوكينغ واحصل على أفضل الأسعار لتذاك"",
                            ""link"": ""http://links.routesme.com/A312529868""
                        }
                    },
                    {
                        ""contentId"": ""A913918025"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/d078c23a-7be3-4c59-b886-395b5ad5c443.mp4"",
                        ""resourceNumber"": ""A0044"",
                        ""name"": ""Virgin Video NewSim Ar"",
                        ""tintColor"": 15009801
                    },
                    {
                        ""contentId"": ""A2048824975"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/Virgin%20Video%20NewSim%20Eng.mp4"",
                        ""resourceNumber"": ""A0042"",
                        ""name"": ""Virgin Video NewSim"",
                        ""tintColor"": 15468295
                    }
                    {
                        ""contentId"": ""A2137220545"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/740de07d-5073-4966-bf43-7f026e8d59c9.mp4"",
                        ""resourceNumber"": ""A004"",
                        ""name"": ""I Save 2"",
                        ""tintColor"": 16743168,
                        ""promotion"": {
                            ""promotionId"": ""A1168841632"",
                            ""title"": ""Get instant discounts, download now!"",
                            ""subtitle"": ""احصل على خصومات فورية.. حمل التطبيق الآن"",
                            ""link"": ""http://links.routesme.com/A1168841632""
                        }
                    },
                    {
                        ""contentId"": ""A1046511380"",
                        ""type"": ""video"",
                        ""url"": ""https://routesme.blob.core.windows.net/advertisements/1a256034-b605-49ed-bddd-cf1a14b99314.mp4"",
                        ""resourceNumber"": ""A0019"",
                        ""name"": ""Turkish Grill"",
                        ""tintColor"": 14091268,
                        ""promotion"": {
                            ""promotionId"": ""A33934682"",
                            ""title"": ""Order online now! Scan the code"",
                            ""subtitle"": ""أطلب اونلاين الان..قم بنسخ الكود"",
                            ""link"": ""http://links.routesme.com/A33934682""
                        }
                    }
                ],
                ""status"": true,
                ""message"": ""Contents retrived successfully."",
                ""statusCode"": 200
            }");
        }

        // public dynamic GetContents(string advertisementId, Pagination pageInfo,string institutionId)
        // {
        //     int totalCount = 0;
        //     try
        //     {
        //         ContentsGetResponse response = new ContentsGetResponse();
        //         List<AdvertisementsForContentModel> contentsModelList = new List<AdvertisementsForContentModel>();
        //         MediasModel medias = new MediasModel();
        //         List<ContentsModel> contents = new List<ContentsModel>();

        //         List<Advertisements> list = _context.Advertisements.ToList();

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