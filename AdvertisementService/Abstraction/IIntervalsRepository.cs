using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;

namespace AdvertisementService.Abstraction
{
    public interface IIntervalsRepository
    {
        dynamic GetIntervals(int intervalId, Pagination pageInfo);
        dynamic UpdateIntervals(IntervalsModel model);
        dynamic DeleteIntervals(int id);
        dynamic InsertIntervals(IntervalsModel model);
    }
}
