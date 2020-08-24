using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Repository
{
    public class IncludeAdvertisements : IIncludeAdvertisements
    {
        private readonly AppSettings _appSettings;
        private readonly advertisementserviceContext _context;
        public IncludeAdvertisements(IOptions<AppSettings> appSettings, advertisementserviceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic GetInstitutionsIncludedData(List<AdvertisementsModel> advertisementsModel)
        {
            List<InstitutionsModel> institutions = new List<InstitutionsModel>();
            foreach (var item in advertisementsModel)
            {
                var client = new RestClient(_appSettings.InstitutionEndpointUrl + item.InstitutionId);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var institutionsData = JsonConvert.DeserializeObject<InstitutionGetResponse>(result);
                    institutions.AddRange(institutionsData.data);
                }
            }
            var usersJson = JsonConvert.SerializeObject(institutions,
                                   new JsonSerializerSettings
                                   {
                                       NullValueHandling = NullValueHandling.Ignore,
                                   });

            return JArray.Parse(usersJson);
        }

        public dynamic GetMediasIncludedData(List<AdvertisementsModel> advertisementsModel)
        {
            List<GetMediasModel> medias = new List<GetMediasModel>();
            foreach (var item in advertisementsModel)
            {
                var mediasDetails = (from media in _context.Medias
                                     join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                     where media.MediaId == item.MediaId
                                     select new GetMediasModel()
                                     {
                                         MediaId = media.MediaId,
                                         CreatedAt = media.CreatedAt,
                                         Url = media.Url,
                                         MediaType = media.MediaType,
                                         Duration = metadata.Duration,
                                         Size = metadata.Size
                                     }).ToList().FirstOrDefault();
                
                if (mediasDetails != null)
                    if (medias.Where(x => x.MediaId == mediasDetails.MediaId).FirstOrDefault() == null)
                        medias.Add(mediasDetails);

            }
            var modelsJson = JsonConvert.SerializeObject(medias,
                                  new JsonSerializerSettings
                                  {
                                      NullValueHandling = NullValueHandling.Ignore,
                                  });

            return JArray.Parse(modelsJson);
        }
    }
}
