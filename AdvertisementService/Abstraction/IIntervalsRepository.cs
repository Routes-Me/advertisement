using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;

namespace AdvertisementService.Abstraction
{
    public interface IIntervalsRepository
    {
        dynamic GetIntervals(string intervalId, Pagination pageInfo);
        dynamic UpdateIntervals(IntervalsModel model);
        dynamic DeleteIntervals(string id);
        dynamic InsertIntervals(IntervalsModel model);
    }
}
