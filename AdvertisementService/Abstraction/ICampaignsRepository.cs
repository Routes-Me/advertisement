using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface ICampaignsRepository
    {
        dynamic GetAdvertisementsAsync(string campaignId, string advertisementsId, string includeType, string embed, string sort_by, Pagination pageInfo);
        dynamic GetCampaigns(string campaignId, string includeType, Pagination pageInfo);
        dynamic UpdateCampaigns(CampaignsModel model);
        dynamic DeleteCampaigns(string id);
        dynamic InsertCampaigns(CampaignsModel model);
        dynamic CreateBroadcasts(string campaignId, BroadcastsDto broadcastsDto);
        dynamic DeleteBroadcasts(string campaignId, string broadcastId);
    }
}
