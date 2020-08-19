using AdvertisementService.Abstraction;
using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class CampaignsRepository : ICampaignsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisements _includeAdvertisements;
        public CampaignsRepository(advertisementserviceContext context, IIncludeAdvertisements includeAdvertisements)
        {
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public CampaignsResponse DeleteCampaigns(int id)
        {
            CampaignsResponse response = new CampaignsResponse();
            try
            {
                var CampaignData = _context.Campaigns.Where(x => x.CampaignId == id).FirstOrDefault();
                if (CampaignData == null)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementscampaigns = _context.Advertisementscampaigns.Where(x => x.AdvertisementId == id).FirstOrDefault();
                if (advertisementscampaigns != null)
                    _context.Advertisementscampaigns.Remove(advertisementscampaigns);

                _context.Campaigns.Remove(CampaignData);
                _context.SaveChanges();
                response.status = true;
                response.message = "Campaign deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Campaigns. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public AdvertisementsGetResponse GetAdvertisements(int campaignId, int advertisementsId, string includeType, PageInfo pageInfo)
        {
            AdvertisementsGetResponse response = new AdvertisementsGetResponse();
            AdvertisementsDetails advertisementsDetails = new AdvertisementsDetails();
            int totalCount = 0;
            try
            {
                List<AdvertisementsModel> advertisementsModelList = new List<AdvertisementsModel>();
                
                if (campaignId == 0)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var campaignCount = _context.Campaigns.Where(x => x.CampaignId == campaignId).ToList().Count();
                if (campaignCount == 0)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                if (advertisementsId == 0)
                {
                    advertisementsModelList = (from campaign in _context.Campaigns
                                               join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                               join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                               where campaign.CampaignId == campaignId
                                               select new AdvertisementsModel()
                                               {
                                                   AdvertisementId = advertisement.AdvertisementId,
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = advertisement.InstitutionId,
                                                   MediaId = advertisement.MediaId
                                               }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.currentPage - 1) * pageInfo.pageSize).Take(pageInfo.pageSize).ToList();


                    totalCount = (from campaign in _context.Campaigns
                                  join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                  join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                  where campaign.CampaignId == campaignId
                                  select new AdvertisementsModel()
                                  {
                                      AdvertisementId = advertisement.AdvertisementId,
                                      CreatedAt = advertisement.CreatedAt,
                                      InstitutionId = advertisement.InstitutionId,
                                      MediaId = advertisement.MediaId
                                  }).ToList().Count();
                }
                else
                {
                    advertisementsModelList = (from campaign in _context.Campaigns
                                               join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                               join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                               where campaign.CampaignId == campaignId && advertisement.AdvertisementId == advertisementsId
                                               select new AdvertisementsModel()
                                               {
                                                   AdvertisementId = advertisement.AdvertisementId,
                                                   CreatedAt = advertisement.CreatedAt,
                                                   InstitutionId = advertisement.InstitutionId,
                                                   MediaId = advertisement.MediaId
                                               }).OrderBy(a => a.AdvertisementId).Skip((pageInfo.currentPage - 1) * pageInfo.pageSize).Take(pageInfo.pageSize).ToList();


                    totalCount = (from campaign in _context.Campaigns
                                  join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                  join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                  where campaign.CampaignId == campaignId && advertisement.AdvertisementId == advertisementsId
                                  select new AdvertisementsModel()
                                  {
                                      AdvertisementId = advertisement.AdvertisementId,
                                      CreatedAt = advertisement.CreatedAt,
                                      InstitutionId = advertisement.InstitutionId,
                                      MediaId = advertisement.MediaId
                                  }).ToList().Count();
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

                advertisementsDetails.advertisements = advertisementsModelList;
                var page = new Pagination
                {
                    offset = pageInfo.currentPage,
                    limit = pageInfo.pageSize,
                    total = totalCount
                };

                response.status = true;
                response.message = "Campaign data retrived successfully.";
                response.included = includeData;
                response.pagination = page;
                response.data = advertisementsDetails;
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

        public CampaignsGetResponse GetCampaigns(int campaignId, string includeType, PageInfo pageInfo)
        {
            CampaignsGetResponse response = new CampaignsGetResponse();
            CampaignsDetails campaignsDetails = new CampaignsDetails();
            int totalCount = 0;
            try
            {
                List<CampaignsModel> campaignsModelList = new List<CampaignsModel>();
                if (campaignId == 0)
                {
                    campaignsModelList = (from campaign in _context.Campaigns
                                             select new CampaignsModel()
                                             {
                                                 CampaignId = campaign.CampaignId,
                                                 Title = campaign.Title,
                                                 StartAt = campaign.StartAt,
                                                 EndAt = campaign.EndAt,
                                                 Status = campaign.Status
                                             }).OrderBy(a => a.CampaignId).Skip((pageInfo.currentPage - 1) * pageInfo.pageSize).Take(pageInfo.pageSize).ToList();

                    totalCount = _context.Campaigns.ToList().Count();
                }
                else
                {
                    campaignsModelList = (from campaign in _context.Campaigns
                                             where campaign.CampaignId == campaignId
                                             select new CampaignsModel()
                                             {
                                                 CampaignId = campaign.CampaignId,
                                                 Title = campaign.Title,
                                                 StartAt = campaign.StartAt,
                                                 EndAt = campaign.EndAt,
                                                 Status = campaign.Status
                                             }).OrderBy(a => a.CampaignId).Skip((pageInfo.currentPage - 1) * pageInfo.pageSize).Take(pageInfo.pageSize).ToList();

                    totalCount = _context.Campaigns.Where(x => x.CampaignId == campaignId).ToList().Count();
                }

                if (campaignsModelList == null || campaignsModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                campaignsDetails.campaigns = campaignsModelList;
                var page = new Pagination
                {
                    offset = pageInfo.currentPage,
                    limit = pageInfo.pageSize,
                    total = totalCount
                };

                response.status = true;
                response.message = "Campaign data retrived successfully.";
                response.pagination = page;
                response.data = campaignsDetails;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching data. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public CampaignsResponse InsertCampaigns(CampaignsModel model)
        {
            CampaignsResponse response = new CampaignsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                Campaigns objCampaigns = new Campaigns()
                {
                    Title = model.Title,
                    StartAt = model.StartAt,
                    EndAt = model.EndAt,
                    Status = model.Status
                };
                _context.Campaigns.Add(objCampaigns);
                _context.SaveChanges();

                response.status = true;
                response.message = "Campaign inserted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Campaigns. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public CampaignsResponse UpdateCampaigns(CampaignsModel model)
        {
            CampaignsResponse response = new CampaignsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var campaignData = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).FirstOrDefault();
                if (campaignData == null)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                campaignData.StartAt = model.StartAt;
                campaignData.EndAt = model.EndAt;
                campaignData.Title = model.Title;
                campaignData.Status = model.Status;
                _context.Campaigns.Update(campaignData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Campaign updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Campaign. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
