using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Helper.Models;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AdvertisementService.Models.Common;
using Obfuscation;
using System;
using System.Collections.Generic;
using System.Linq;
namespace AdvertisementService.Helper.Repository
{
    public class CommonFunctions : ICommonFunctions
    {
        private readonly advertisementserviceContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        private readonly AdvertisementService.Models.Common.AppSettings _appSettings;

        public CommonFunctions(IOptions<AdvertisementService.Models.Common.AppSettings> appSettings, advertisementserviceContext context, IIncludeAdvertisementsRepository includeAdvertisements)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public List<AdvertisementsGetModel> GetAdvertisementWithCampaigns(List<AdvertisementsGetModel> advertisementsModelList)
        {
            var promotions = _includeAdvertisements.GetPromotionsData();
            List<AdvertisementsGetModel> advertisementsList = new List<AdvertisementsGetModel>();
            foreach (var item in advertisementsModelList)
            {
                AdvertisementsGetModel advertisements = new AdvertisementsGetModel();
                advertisements.AdvertisementId = item.AdvertisementId;
                advertisements.CreatedAt = item.CreatedAt;
                advertisements.InstitutionId = item.InstitutionId;
                advertisements.MediaId = item.MediaId;
                advertisements.ResourceName = item.ResourceName;
                var campaignList = (from advertisement in _context.Advertisements
                                    join advertisementsCampaigns in _context.AdvertisementsCampaigns on advertisement.AdvertisementId equals advertisementsCampaigns.AdvertisementId
                                    join campaigns in _context.Campaigns on advertisementsCampaigns.CampaignId equals campaigns.CampaignId
                                    where advertisement.AdvertisementId == ObfuscationClass.DecodeId(Convert.ToInt32(item.AdvertisementId), _appSettings.PrimeInverse)
                                    select new
                                    {
                                        campaignId = ObfuscationClass.EncodeId(Convert.ToInt32(campaigns.CampaignId), _appSettings.Prime).ToString()
                                    }).ToList();

                List<string> lstItems = new List<string>();
                foreach (var innerItem in campaignList)
                {
                    lstItems.Add(innerItem.campaignId);
                }
                advertisements.CampaignId = lstItems;
                advertisements.IntervalId = item.IntervalId;
                advertisements.PromotionsId = promotions.Where(x => x.AdvertisementId == item.AdvertisementId).Select(x => x.PromotionId).FirstOrDefault();
                advertisementsList.Add(advertisements);
            }
            advertisementsModelList = new List<AdvertisementsGetModel>();
            advertisementsModelList = advertisementsList;
            return advertisementsModelList;
        }

        public List<AdvertisementsGetModel> GetAllAdvertisements(List<Advertisements> advertisements, List<AdvertisementsIntervals> advertisementsCampaignsData, Pagination pageInfo)
        {
            return (from advertisement in advertisements
                    join advertisementsIntervals in advertisementsCampaignsData on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId into Details
                    from m in Details.DefaultIfEmpty()
                    select new AdvertisementsGetModel()
                    {
                        AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                        CreatedAt = advertisement.CreatedAt,
                        InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                        MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                        ResourceName = advertisement.ResourceName,
                        IntervalId = m == null ? null : ObfuscationClass.EncodeId(Convert.ToInt32(m.IntervalId), _appSettings.Prime).ToString(),
                    }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
        }
    }
}
