using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class AdvertisementsRepository : IAdvertisementsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisements _includeAdvertisements;
        public AdvertisementsRepository(advertisementserviceContext context, IIncludeAdvertisements includeAdvertisements)
        {
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public AdvertisementsResponse DeleteAdvertisements(int id)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            try
            {
                var advertisementsData = _context.Advertisements.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementsData == null)
                {
                    response.status = false;
                    response.message = "Advertisement not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementsIntervals = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementsIntervals != null)
                    _context.AdvertisementsIntervals.Remove(advertisementsIntervals);

                var advertisementscampaigns = _context.AdvertisementsCampaigns.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementscampaigns != null)
                    _context.AdvertisementsCampaigns.Remove(advertisementscampaigns);

                _context.Advertisements.Remove(advertisementsData);
                _context.SaveChanges();
                response.status = true;
                response.message = "Advertisements deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Advertisement. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsGetResponse GetAdvertisements(int advertisementId, string includeType, Pagination pageInfo)
        {
            AdvertisementsGetResponse response = new AdvertisementsGetResponse();
            int totalCount = 0;
            try
            {
                List<AdvertisementsModel> advertisementsModelList = new List<AdvertisementsModel>();
                
                if (advertisementId == 0)
                {
                    advertisementsModelList = (from advertisement in _context.Advertisements
                                               select new AdvertisementsModel()
                                               {
                                                   AdvertisementId = advertisement.AdvertisementId,
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = advertisement.InstitutionId,
                                                   MediaId = advertisement.MediaId,
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
                                               }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Advertisements.Where(x => x.AdvertisementId == advertisementId).ToList().Count();
                }

                if (advertisementsModelList == null || advertisementsModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Advertisements not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
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
                response.message = "Campaign data retrived successfully.";
                response.included = includeData;
                response.pagination = page;
                response.data = advertisementsModelList;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching campaign. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsResponse InsertAdvertisements(PostAdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var media = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (media == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var interval = _context.Intervals.Where(x => x.IntervalId == model.IntervalId).FirstOrDefault();
                if (interval == null)
                {
                    response.status = false;
                    response.message = "Interval not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var campaign = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).FirstOrDefault();
                if (interval == null)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                Advertisements objAdvertisements = new Advertisements()
                {
                    CreatedAt = DateTime.UtcNow,
                    InstitutionId = model.InstitutionId,
                    MediaId = model.MediaId
                };
                _context.Advertisements.Add(objAdvertisements);
                _context.SaveChanges();

                AdvertisementsIntervals advertisementsintervals = new AdvertisementsIntervals()
                {
                    AdvertisementId = objAdvertisements.AdvertisementId,
                    IntervalId = interval.IntervalId
                };
                _context.AdvertisementsIntervals.Add(advertisementsintervals);

                AdvertisementsCampaigns objAdvertisementscampaigns = new AdvertisementsCampaigns()
                {
                    AdvertisementId = objAdvertisements.AdvertisementId,
                    CampaignId = campaign.CampaignId
                };
                _context.AdvertisementsCampaigns.Add(objAdvertisementscampaigns);
                _context.SaveChanges();

                response.status = true;
                response.message = "Advertisements inserted successfully.";
                response.responseCode = ResponseCode.Created;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Advertisement. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsResponse UpdateAdvertisements(PostAdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var advertisement = _context.Advertisements.Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
                if (advertisement == null)
                {
                    response.status = false;
                    response.message = "Advertisement not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var media = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (media == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var interval = _context.Intervals.Where(x => x.IntervalId == model.IntervalId).FirstOrDefault();
                if (interval == null)
                {
                    response.status = false;
                    response.message = "Interval not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var campaign = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).FirstOrDefault();
                if (interval == null)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementsintervals = _context.AdvertisementsIntervals.Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
                if (advertisementsintervals == null)
                {
                    AdvertisementsIntervals objAdvertisementsintervals = new AdvertisementsIntervals()
                    {
                        AdvertisementId = advertisement.AdvertisementId,
                        IntervalId = interval.IntervalId
                    };
                    _context.AdvertisementsIntervals.Add(advertisementsintervals);
                }
                else
                {
                    advertisementsintervals.AdvertisementId = advertisement.AdvertisementId;
                    advertisementsintervals.IntervalId = interval.IntervalId;
                    _context.AdvertisementsIntervals.Update(advertisementsintervals);
                }

                var advertisementscampaigns = _context.AdvertisementsCampaigns.Where(x => x.AdvertisementId == model.AdvertisementId).FirstOrDefault();
                if (advertisementscampaigns == null)
                {
                    AdvertisementsCampaigns objAdvertisementsintervals = new AdvertisementsCampaigns()
                    {
                        CampaignId = campaign.CampaignId,
                        AdvertisementId = advertisement.AdvertisementId
                    };
                    _context.AdvertisementsCampaigns.Add(objAdvertisementsintervals);
                }
                else
                {
                    advertisementscampaigns.AdvertisementId = advertisement.AdvertisementId;
                    advertisementscampaigns.CampaignId = campaign.CampaignId;
                    _context.AdvertisementsCampaigns.Update(advertisementscampaigns);
                }

                advertisement.InstitutionId = model.InstitutionId;
                advertisement.MediaId = model.MediaId;
                _context.Advertisements.Update(advertisement);
                _context.SaveChanges();

                response.status = true;
                response.message = "Advertisement updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Advertisement. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
