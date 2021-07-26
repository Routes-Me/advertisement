using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Helper.Models;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AdvertisementService.Models.Common;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementService.Helper.Repository
{
    public class CommonFunctions : ICommonFunctions
    {
        private readonly AdvertisementContext _context;
        private readonly IIncludeAdvertisementsRepository _includeAdvertisements;
        private readonly AdvertisementService.Models.Common.AppSettings _appSettings;

        public CommonFunctions(IOptions<AdvertisementService.Models.Common.AppSettings> appSettings, AdvertisementContext context, IIncludeAdvertisementsRepository includeAdvertisements)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _includeAdvertisements = includeAdvertisements;
        }

        public List<AdvertisementsGetModel> GetAdvertisementWithCampaigns(List<AdvertisementsGetModelWithCampaign> advertisementsModelListWithCampaign)
        {
            var promotions = _includeAdvertisements.GetPromotionsData();
            List<AdvertisementsGetModel> advertisementsList = new List<AdvertisementsGetModel>();
            foreach (var item in advertisementsModelListWithCampaign)
            {
                AdvertisementsGetModel advertisements = new AdvertisementsGetModel();
                advertisements.AdvertisementId = item.AdvertisementId;
                advertisements.CreatedAt = item.CreatedAt;
                advertisements.InstitutionId = item.InstitutionId;
                advertisements.MediaId = item.MediaId;
                advertisements.ResourceNumber = item.ResourceNumber;
                advertisements.Name = item.Name;
                List<string> lstItems = new List<string>();
                foreach (var innerItem in item.Campaigns)
                {
                    lstItems.Add(Obfuscation.Encode(innerItem.CampaignId));
                }
                advertisements.CampaignId = lstItems;
                advertisements.IntervalId = item.IntervalId;
                advertisements.PromotionsId = promotions.Where(x => x.AdvertisementId == item.AdvertisementId).Select(x => x.PromotionId).FirstOrDefault();
                advertisements.TintColor = item.TintColor;
                advertisements.InvertedTintColor = item.InvertedTintColor;
                advertisementsList.Add(advertisements);
            }
            var advertisementsModelList = new List<AdvertisementsGetModel>();
            advertisementsModelList = advertisementsList;
            return advertisementsModelList;
        }

        public List<AdvertisementsGetModelWithCampaign> GetAllAdvertisements(List<Advertisements> advertisements, List<AdvertisementsIntervals> BroadcastsData, Pagination pageInfo)
        {
            return (from advertisement in advertisements
                    join advertisementsIntervals in BroadcastsData on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId into Details
                    from m in Details.DefaultIfEmpty()
                    select new AdvertisementsGetModelWithCampaign()
                    {
                        AdvertisementId = Obfuscation.Encode(advertisement.AdvertisementId),
                        CreatedAt = advertisement.CreatedAt,
                        InstitutionId = Obfuscation.Encode(Convert.ToInt32(advertisement.InstitutionId)),
                        MediaId = Obfuscation.Encode(Convert.ToInt32(advertisement.MediaId)),
                        ResourceNumber = advertisement.ResourceNumber,
                        Name = advertisement.Name,
                        IntervalId = m == null ? null : Obfuscation.Encode(m.IntervalId),
                        Campaigns = advertisement.Broadcasts.ToList(),
                        TintColor = advertisement.TintColor,
                        InvertedTintColor = advertisement.InvertedTintColor
                    }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
        }
    }
}
