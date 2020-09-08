using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IMediasRepository
    {
        dynamic GetMedias(int mediaId, string include, Pagination pageInfo);
        Task<dynamic> UpdateMedias(MediasModel model);
        Task<dynamic> DeleteMedias(int id);
        Task<dynamic> InsertMedias(MediasModel model);
    }
}
