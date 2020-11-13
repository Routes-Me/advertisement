using AdvertisementService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Abstraction
{
    public interface IVideoConversionRepository
    {
        Task<VideoMetadata> ConvertVideoAsync(string filepath);
    }
}
