using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IIntervalsRepository
    {
        IntervalsResponse GetIntervals(GetIntervalsModel model);
        IntervalsResponse UpdateIntervals(IntervalsModel model);
        IntervalsResponse DeleteIntervals(int id);
        IntervalsResponse InsertIntervals(IntervalsModel model);
    }
}
