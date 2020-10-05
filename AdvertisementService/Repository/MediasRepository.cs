using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class MediasRepository : IMediasRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly AzureStorageBlobConfig _config;
        public MediasRepository(IOptions<AzureStorageBlobConfig> config, advertisementserviceContext context)
        {
            _config = config.Value;
            _context = context;
        }

        public async Task<dynamic> DeleteMedias(string id)
        {
            try
            {
                var medias = _context.Medias.Include(x => x.Advertisements).Include(x => x.MediaMetadata).Where(x => x.MediaId == Convert.ToInt32(id)).FirstOrDefault();
                if (medias == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                var advertisementData = medias.Advertisements.Where(x => x.MediaId == Convert.ToInt32(id)).FirstOrDefault();
                if (advertisementData != null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaAssociatedWithAdvertisement, StatusCodes.Status409Conflict);

                var mediaReferenceName = medias.Url.Split('/');
                if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = BlobClient.GetContainerReference(_config.Container);
                    if (await container.ExistsAsync())
                    {
                        CloudBlob file = container.GetBlobReference(mediaReferenceName.LastOrDefault());
                        if (await file.ExistsAsync())
                            await file.DeleteAsync();
                    }
                }

                _context.MediaMetadata.Remove(medias.MediaMetadata);
                _context.Medias.Remove(medias);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.MediaDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetMedias(string mediaId, string includeType, Pagination pageInfo)
        {
            MediasGetResponse response = new MediasGetResponse();
            int totalCount = 0;
            try
            {
                List<GetMediasModel> mediasModelList = new List<GetMediasModel>();
                if (mediaId == "0")
                {
                    mediasModelList = (from media in _context.Medias
                                       join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                       select new GetMediasModel()
                                       {
                                           MediaId = media.MediaId.ToString(),
                                           CreatedAt = media.CreatedAt,
                                           Url = media.Url,
                                           MediaType = media.MediaType,
                                           Duration = metadata.Duration,
                                           Size = metadata.Size
                                       }).OrderBy(a => a.MediaId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  select new GetMediasModel() { }).ToList().Count();
                }
                else
                {
                    mediasModelList = (from media in _context.Medias
                                       join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                       where media.MediaId == Convert.ToInt32(mediaId)
                                       select new GetMediasModel()
                                       {
                                           MediaId = media.MediaId.ToString(),
                                           CreatedAt = media.CreatedAt,
                                           Url = media.Url,
                                           MediaType = media.MediaType,
                                           Duration = metadata.Duration,
                                           Size = metadata.Size
                                       }).OrderBy(a => a.MediaId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  where media.MediaId == Convert.ToInt32(mediaId)
                                  select new GetMediasModel() { }).ToList().Count();
                }


                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.MediaRetrived;
                response.pagination = page;
                response.data = mediasModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> InsertMedias(MediasModel model)
        {
            string blobUrl = string.Empty;
            MediasInsertResponse response = new MediasInsertResponse();
            try
            {
                string mediaReferenceName = model.media.FileName.Split('.')[0] + "_" + DateTime.UtcNow.Ticks + "." + model.media.FileName.Split('.')[1];
                if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                    await blockBlob.UploadFromStreamAsync(model.media.OpenReadStream());
                    blobUrl = blockBlob.Uri.AbsoluteUri;
                }

                MediaMetadata mediaMetadata = new MediaMetadata()
                {
                    Duration = model.Duration,
                    Size = model.Size
                };
                _context.MediaMetadata.Add(mediaMetadata);
                _context.SaveChanges();

                Medias media = new Medias()
                {
                    Url = blobUrl,
                    CreatedAt = model.CreatedAt,
                    MediaType = model.MediaType,
                    MediaMetadataId = mediaMetadata.MediaMetadataId
                };
                _context.Medias.Add(media);
                _context.SaveChanges();

                response.status = true;
                response.statusCode = StatusCodes.Status201Created;
                response.message = CommonMessage.MediaInsert;
                response.mediaId = media.MediaId.ToString();
                response.url = media.Url;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<dynamic> UpdateMedias(MediasModel model)
        {
            string blobUrl = string.Empty, mediaReferenceName = string.Empty;
            try
            {
                var mediaData = _context.Medias.Include(x => x.MediaMetadata).Where(x => x.MediaId == Convert.ToInt32(model.MediaId)).FirstOrDefault();
                if (mediaData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.MediaNotFound, StatusCodes.Status404NotFound);

                if (!string.IsNullOrEmpty(mediaData.Url))
                {
                    var existingMediaReferenceName = mediaData.Url.Split('/');
                    if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                    {
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                        if (await container.ExistsAsync())
                        {
                            CloudBlob file = container.GetBlobReference(existingMediaReferenceName.LastOrDefault());
                            if (await file.ExistsAsync())
                                await file.DeleteAsync();
                        }

                        mediaReferenceName = model.media.FileName.Split('.')[0] + "_" + DateTime.UtcNow.Ticks + "." + model.media.FileName.Split('.')[1];
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                        await blockBlob.UploadFromStreamAsync(model.media.OpenReadStream());
                        blobUrl = blockBlob.Uri.AbsoluteUri;
                    }
                }
                else
                {
                    mediaReferenceName = model.media.FileName.Split('.')[0] + "_" + DateTime.UtcNow.Ticks + "." + model.media.FileName.Split('.')[1];
                    if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                    {
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                        await blockBlob.UploadFromStreamAsync(model.media.OpenReadStream());
                        blobUrl = blockBlob.Uri.AbsoluteUri;
                    }
                }

                if (mediaData.MediaMetadata == null)
                {
                    MediaMetadata mediaMetadata = new MediaMetadata()
                    {
                        Duration = model.Duration,
                        Size = model.Size
                    };
                    _context.MediaMetadata.Add(mediaMetadata);
                    _context.SaveChanges();
                    mediaData.MediaMetadataId = mediaMetadata.MediaMetadataId;
                }
                else
                {
                    mediaData.MediaMetadata.Duration = model.Duration;
                    mediaData.MediaMetadata.Size = model.Size;
                    _context.MediaMetadata.Update(mediaData.MediaMetadata);
                    _context.SaveChanges();
                }

                mediaData.Url = blobUrl;
                mediaData.CreatedAt = model.CreatedAt;
                mediaData.MediaType = model.MediaType;
                _context.Medias.Update(mediaData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.MediaUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
