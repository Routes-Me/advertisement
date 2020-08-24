using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface ICampaignsRepository
    {
        AdvertisementsGetResponse GetAdvertisements(int campaignId, int advertisementsId, string includeType, Pagination pageInfo);
        CampaignsGetResponse GetCampaigns(int campaignId, string includeType, Pagination pageInfo);
        CampaignsResponse UpdateCampaigns(CampaignsModel model);
        CampaignsResponse DeleteCampaigns(int id);
        CampaignsResponse InsertCampaigns(CampaignsModel model);
        ActiveCampAdWithQRGetResponse GetAdvertisementsofActiveCampaign(string include, Pagination pageInfo);
    }
}
