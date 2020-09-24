using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;

namespace AdvertisementService.Abstraction
{
    public interface ICampaignsRepository
    {
        dynamic GetAdvertisements(string campaignId, string advertisementsId, string includeType, Pagination pageInfo);
        dynamic GetCampaigns(string campaignId, string includeType, Pagination pageInfo);
        dynamic UpdateCampaigns(CampaignsModel model);
        dynamic DeleteCampaigns(string id);
        dynamic InsertCampaigns(CampaignsModel model);
    }
}
