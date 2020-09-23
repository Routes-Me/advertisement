using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;

namespace AdvertisementService.Abstraction
{
    public interface IAdvertisementsRepository
    {
        dynamic GetAdvertisements(int institutionId, int advertisementId, string includeType, Pagination pageInfo);
        dynamic UpdateAdvertisements(PostAdvertisementsModel model);
        dynamic DeleteAdvertisements(int id);
        dynamic InsertAdvertisements(PostAdvertisementsModel model);
    }
}
