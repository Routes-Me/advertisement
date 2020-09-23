using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;

namespace AdvertisementService.Abstraction
{
    public interface ICampaignsRepository
    {
        dynamic GetAdvertisements(int campaignId, int advertisementsId, string includeType, Pagination pageInfo);
        dynamic GetCampaigns(int campaignId, string includeType, Pagination pageInfo);
        dynamic UpdateCampaigns(CampaignsModel model);
        dynamic DeleteCampaigns(int id);
        dynamic InsertCampaigns(CampaignsModel model);
        dynamic GetAdvertisementsofActiveCampaign(string include, Pagination pageInfo);
    }
}
