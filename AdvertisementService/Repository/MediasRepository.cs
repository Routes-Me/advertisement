using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<MediasResponse> DeleteMedias(int id)
        {
            MediasResponse response = new MediasResponse();
            try
            {
                var medias = _context.Medias.Where(x => x.MediaId == id).FirstOrDefault();
                if (medias == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementData = _context.Advertisements.Where(x => x.MediaId == id).FirstOrDefault();
                if (advertisementData != null)
                {
                    response.status = false;
                    response.message = "Media is associated with other advertisemnts.";
                    response.responseCode = ResponseCode.Conflict;
                    return response;
                }

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

                var metaData = _context.MediaMetadata.Where(x => x.MediaMetadataId == medias.MediaMetadataId).FirstOrDefault();
                if (metaData != null)
                    _context.MediaMetadata.Remove(metaData);

                _context.Medias.Remove(medias);
                _context.SaveChanges();
                response.status = true;
                response.message = "Media deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Media. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public MediasGetResponse GetMedias(int mediaId, string includeType, Pagination pageInfo)
        {
            MediasGetResponse response = new MediasGetResponse();
            int totalCount = 0;
            try
            {
                List<GetMediasModel> mediasModelList = new List<GetMediasModel>();
                if (mediaId == 0)
                {
                    mediasModelList = (from media in _context.Medias
                                       join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                       select new GetMediasModel()
                                       {
                                           MediaId = media.MediaId,
                                           CreatedAt = media.CreatedAt,
                                           Url = media.Url,
                                           MediaType = media.MediaType,
                                           Duration = metadata.Duration,
                                           Size = metadata.Size
                                       }).OrderBy(a => a.MediaId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  select new GetMediasModel()
                                  {
                                      MediaId = media.MediaId,
                                      CreatedAt = media.CreatedAt,
                                      Url = media.Url,
                                      MediaType = media.MediaType,
                                      Duration = metadata.Duration,
                                      Size = metadata.Size
                                  }).ToList().Count();
                }
                else
                {
                    mediasModelList = (from media in _context.Medias
                                       join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                       where media.MediaId == mediaId
                                       select new GetMediasModel()
                                       {
                                           MediaId = media.MediaId,
                                           CreatedAt = media.CreatedAt,
                                           Url = media.Url,
                                           MediaType = media.MediaType,
                                           Duration = metadata.Duration,
                                           Size = metadata.Size
                                       }).OrderBy(a => a.MediaId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  where media.MediaId == mediaId
                                  select new GetMediasModel()
                                  {
                                      MediaId = media.MediaId,
                                      CreatedAt = media.CreatedAt,
                                      Url = media.Url,
                                      MediaType = media.MediaType,
                                      Duration = metadata.Duration,
                                      Size = metadata.Size
                                  }).ToList().Count();
                }

                if (mediasModelList == null || mediasModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = "Media data retrived successfully.";
                response.pagination = page;
                response.data = mediasModelList;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching medias. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<MediasResponse> InsertMedias(MediasModel model)
        {
            MediasResponse response = new MediasResponse();
            string blobUrl = string.Empty;
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

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

                Medias objMedia = new Medias()
                {
                    Url = blobUrl,
                    CreatedAt = model.CreatedAt,
                    MediaType = model.MediaType,
                    MediaMetadataId = mediaMetadata.MediaMetadataId
                };
                _context.Medias.Add(objMedia);
                _context.SaveChanges();

                response.status = true;
                response.message = "Media inserted successfully.";
                response.responseCode = ResponseCode.Created;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Media. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public async Task<MediasResponse> UpdateMedias(MediasModel model)
        {
            MediasResponse response = new MediasResponse();
            string blobUrl = string.Empty, mediaReferenceName = string.Empty;
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var mediaData = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (mediaData == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

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

                var metadata = _context.MediaMetadata.Where(x => x.MediaMetadataId == mediaData.MediaMetadataId).FirstOrDefault();
                if (metadata == null)
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
                    metadata.Duration = model.Duration;
                    metadata.Size = model.Size;
                    _context.MediaMetadata.Update(metadata);
                    _context.SaveChanges();
                }

                mediaData.Url = blobUrl;
                mediaData.CreatedAt = model.CreatedAt;
                mediaData.MediaType = model.MediaType;
                _context.Medias.Update(mediaData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Media updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Media. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
