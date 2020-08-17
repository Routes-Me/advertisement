using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class CampaignsRepository : ICampaignsRepository
    {
        private readonly advertisementserviceContext _context;
        public CampaignsRepository(advertisementserviceContext context)
        {
            _context = context;
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
                    response.campaignsDetails = null;
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
                response.campaignsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Campaigns. Error Message - " + ex.Message;
                response.campaignsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public CampaignsResponse GetCampaigns(GetCampaignsModel model)
        {
            CampaignsResponse Response = new CampaignsResponse();
            CampaignsDetails objCampaignsDetails = new CampaignsDetails();
            int totalCampaignCount = 0, totalAdvertisementCount = 0, totalCount = 0;
            try
            {
                List<CampaignsModel> objCampaignsModelList = new List<CampaignsModel>();
                List<AdvertisementsModel> objAdvertisementsModelList = new List<AdvertisementsModel>();
                CampaignsDetailsData campaignsDetailsData = new CampaignsDetailsData();
                if (model.isCampaign)
                {
                    if (model.CampaignId == 0)
                    {
                        objCampaignsModelList = (from campaign in _context.Campaigns
                                                 select new CampaignsModel()
                                                 {
                                                     CampaignId = campaign.CampaignId,
                                                     Title = campaign.Title,
                                                     StartAt = campaign.StartAt,
                                                     EndAt = campaign.EndAt,
                                                     Status = campaign.Status
                                                 }).OrderBy(a => a.CampaignId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                        totalCampaignCount = (from campaign in _context.Campaigns
                                      select new CampaignsModel()
                                      {
                                          CampaignId = campaign.CampaignId,
                                          Title = campaign.Title,
                                          StartAt = campaign.StartAt,
                                          EndAt = campaign.EndAt,
                                          Status = campaign.Status
                                      }).ToList().Count();
                    }
                    else
                    {
                        objCampaignsModelList = (from campaign in _context.Campaigns
                                                 where campaign.CampaignId == model.CampaignId
                                                 select new CampaignsModel()
                                                 {
                                                     CampaignId = campaign.CampaignId,
                                                     Title = campaign.Title,
                                                     StartAt = campaign.StartAt,
                                                     EndAt = campaign.EndAt,
                                                     Status = campaign.Status
                                                 }).OrderBy(a => a.CampaignId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                        totalCampaignCount = (from campaign in _context.Campaigns
                                      where campaign.CampaignId == model.CampaignId
                                      select new CampaignsModel()
                                      {
                                          CampaignId = campaign.CampaignId,
                                          Title = campaign.Title,
                                          StartAt = campaign.StartAt,
                                          EndAt = campaign.EndAt,
                                          Status = campaign.Status
                                      }).ToList().Count();
                    }
                }
                else if (model.isAdvertisement)
                {
                    if (model.CampaignId == 0)
                    {
                        Response.status = false;
                        Response.message = "Campaign not found.";
                        Response.campaignsDetails = null;
                        Response.responseCode = ResponseCode.NotFound;
                        return Response;
                    }
                    else
                    {
                        var campaignCount = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).ToList().Count();
                        if (campaignCount == 0)
                        {
                            Response.status = false;
                            Response.message = "Vehicle not found.";
                            Response.campaignsDetails = null;
                            Response.responseCode = ResponseCode.NotFound;
                            return Response;
                        }
                        else
                        {
                            if (model.AdvertisementId == 0)
                            {
                                objAdvertisementsModelList = (from campaign in _context.Campaigns
                                                              join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                                              join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                                              where campaign.CampaignId == model.CampaignId
                                                              select new AdvertisementsModel()
                                                              {
                                                                  AdvertisementId = advertisement.AdvertisementId,
                                                                  CreatedAt = advertisement.CreatedAt,
                                                                  InstitutionId = advertisement.InstitutionId,
                                                                  MediaId = advertisement.MediaId
                                                              }).OrderBy(a => a.AdvertisementId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();


                                totalAdvertisementCount = (from campaign in _context.Campaigns
                                                          join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                                          join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                                          where campaign.CampaignId == model.CampaignId
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
                                objAdvertisementsModelList = (from campaign in _context.Campaigns
                                                              join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                                              join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                                              where campaign.CampaignId == model.CampaignId && advertisement.AdvertisementId == model.AdvertisementId
                                                              select new AdvertisementsModel()
                                                              {
                                                                  AdvertisementId = advertisement.AdvertisementId,
                                                                  CreatedAt = advertisement.CreatedAt,
                                                                  InstitutionId = advertisement.InstitutionId,
                                                                  MediaId = advertisement.MediaId
                                                              }).OrderBy(a => a.AdvertisementId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();


                                totalAdvertisementCount = (from campaign in _context.Campaigns
                                                           join advertiseincampaign in _context.Advertisementscampaigns on campaign.CampaignId equals advertiseincampaign.CampaignId
                                                           join advertisement in _context.Advertisements on advertiseincampaign.AdvertisementId equals advertisement.AdvertisementId
                                                           where campaign.CampaignId == model.CampaignId && advertisement.AdvertisementId == model.AdvertisementId
                                                           select new AdvertisementsModel()
                                                           {
                                                               AdvertisementId = advertisement.AdvertisementId,
                                                               CreatedAt = advertisement.CreatedAt,
                                                               InstitutionId = advertisement.InstitutionId,
                                                               MediaId = advertisement.MediaId
                                                           }).ToList().Count();
                            }
                        }
                    }
                }

                if (model.isAdvertisement)
                {
                    if (objAdvertisementsModelList == null || objAdvertisementsModelList.Count == 0)
                    {
                        Response.status = false;
                        Response.message = "Advertisements not found.";
                        Response.campaignsDetails = null;
                        Response.responseCode = ResponseCode.NotFound;
                        return Response;
                    }

                    Response.message = "Advertisement data retrived successfully.";
                    campaignsDetailsData.advertisements = objAdvertisementsModelList;
                    totalCount = totalAdvertisementCount;
                }
                else if (model.isCampaign)
                {
                    if (objCampaignsModelList == null || objCampaignsModelList.Count == 0)
                    {
                        Response.status = false;
                        Response.message = "Campaigns not found.";
                        Response.campaignsDetails = null;
                        Response.responseCode = ResponseCode.NotFound;
                        return Response;
                    }
                    Response.message = "Campaigns data retrived successfully.";
                    campaignsDetailsData.campaigns = objCampaignsModelList;
                    totalCount = totalCampaignCount;
                }
                var page = new Pagination
                {
                    TotalCount = totalCount,
                    CurrentPage = model.currentPage,
                    PageSize = model.pageSize,
                    TotalPages = (int)Math.Ceiling(decimal.Divide(totalCount, model.pageSize)),
                    IndexOne = ((model.currentPage - 1) * model.pageSize + 1),
                    IndexTwo = (((model.currentPage - 1) * model.pageSize + model.pageSize) <= totalCount ? ((model.currentPage - 1) * model.pageSize + model.pageSize) : totalCount)
                };

                objCampaignsDetails.data = campaignsDetailsData;
                objCampaignsDetails.pagination = page;
                Response.status = true;
                Response.campaignsDetails = objCampaignsDetails;
                Response.responseCode = ResponseCode.Success;
                return Response;
            }
            catch (Exception ex)
            {
                Response.status = false;
                Response.message = "Something went wrong while fetching data. Error Message - " + ex.Message;
                Response.campaignsDetails = null;
                Response.responseCode = ResponseCode.InternalServerError;
                return Response;
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
                    response.campaignsDetails = null;
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
                response.campaignsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Campaigns. Error Message - " + ex.Message;
                response.campaignsDetails = null;
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
                    response.campaignsDetails = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var campaignData = _context.Campaigns.Where(x => x.CampaignId == model.CampaignId).FirstOrDefault();
                if (campaignData == null)
                {
                    response.status = false;
                    response.message = "Campaign not found.";
                    response.campaignsDetails = null;
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
                response.campaignsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Campaign. Error Message - " + ex.Message;
                response.campaignsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
