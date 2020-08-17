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
        CampaignsResponse GetCampaigns(GetCampaignsModel model);
        CampaignsResponse UpdateCampaigns(CampaignsModel model);
        CampaignsResponse DeleteCampaigns(int id);
        CampaignsResponse InsertCampaigns(CampaignsModel model);
    }
}
