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
        private readonly Dependencies _dependencies;
        public IncludeAdvertisements(IOptions<AppSettings> appSettings, advertisementserviceContext context, IOptions<Dependencies> dependencies)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _dependencies = dependencies.Value;
        }

        public dynamic GetCampaignIncludedData(List<AdvertisementsModel> advertisementsModel)
        {
            List<CampaignsModel> campaigns = new List<CampaignsModel>();
            foreach (var item in advertisementsModel)
            {
                var campaignsDetails = (from campaign in _context.Campaigns
                                     join campadvt in _context.AdvertisementsCampaigns on campaign.CampaignId equals campadvt.CampaignId
                                     join advt in _context.Advertisements on campadvt.AdvertisementId equals advt.AdvertisementId
                                     where advt.AdvertisementId == Convert.ToInt32(item.AdvertisementId)
                                     select new CampaignsModel()
                                     {
                                         CampaignId = campaign.CampaignId.ToString(),
                                         StartAt = campaign.StartAt,
                                         EndAt = campaign.EndAt,
                                         Status = campaign.Status,
                                         Title = campaign.Title,
                                         CreatedAt = campaign.CreatedAt,
                                         UpdatedAt = campaign.UpdatedAt
                                     }).ToList().FirstOrDefault();

                if (campaignsDetails != null)
                    if (campaigns.Where(x => x.CampaignId == campaignsDetails.CampaignId).FirstOrDefault() == null)
                        campaigns.Add(campaignsDetails);

            }
            var modelsJson = JsonConvert.SerializeObject(campaigns,
                                  new JsonSerializerSettings
                                  {
                                      NullValueHandling = NullValueHandling.Ignore,
                                  });

            return JArray.Parse(modelsJson);
        }

        public dynamic GetInstitutionsIncludedData(List<AdvertisementsModel> advertisementsModel)
        {
            List<InstitutionsModel> institutions = new List<InstitutionsModel>();
            foreach (var item in advertisementsModel)
            {
                var client = new RestClient(_appSettings.Host + _dependencies.InstitutionUrl + item.InstitutionId);
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

        public dynamic GetIntervalIncludedData(List<AdvertisementsModel> advertisementsModel)
        {
            List<IntervalsModel> intervals = new List<IntervalsModel>();
            foreach (var item in advertisementsModel)
            {
                var intervalsDetails = (from interval in _context.Intervals
                                        join advtInterval in _context.AdvertisementsIntervals on interval.IntervalId equals advtInterval.IntervalId
                                        join advt in _context.Advertisements on advtInterval.AdvertisementId equals advt.AdvertisementId
                                        where advt.AdvertisementId == Convert.ToInt32(item.AdvertisementId)
                                        select new IntervalsModel()
                                        {
                                            IntervalId = interval.IntervalId.ToString(),
                                            Title = interval.Title
                                        }).ToList().FirstOrDefault();

                if (intervalsDetails != null)
                    if (intervals.Where(x => x.IntervalId == intervalsDetails.IntervalId).FirstOrDefault() == null)
                        intervals.Add(intervalsDetails);

            }
            var modelsJson = JsonConvert.SerializeObject(intervals,
                                  new JsonSerializerSettings
                                  {
                                      NullValueHandling = NullValueHandling.Ignore,
                                  });

            return JArray.Parse(modelsJson);
        }

        public dynamic GetMediasIncludedData(List<AdvertisementsModel> advertisementsModel)
        {
            List<GetMediasModel> medias = new List<GetMediasModel>();
            foreach (var item in advertisementsModel)
            {
                var mediasDetails = (from media in _context.Medias
                                     join metadata in _context.MediaMetadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                     where media.MediaId == Convert.ToInt32(item.MediaId)
                                     select new GetMediasModel()
                                     {
                                         MediaId = media.MediaId.ToString(),
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
