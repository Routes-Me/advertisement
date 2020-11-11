﻿using AdvertisementService.Helper.Abstraction;
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
using Microsoft.EntityFrameworkCore;

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
                advertisements.ResourceName = item.ResourceName;
                //int AdvertisementIdDecoded = ObfuscationClass.DecodeId(Convert.ToInt32(item.AdvertisementId), _appSettings.PrimeInverse);
                //var campaignList = _context.Advertisements.Include(x => x.AdvertisementsCampaigns).Where(x => x.AdvertisementId == AdvertisementIdDecoded).Select(x => x.AdvertisementsCampaigns).FirstOrDefault();
                List<string> lstItems = new List<string>();
                foreach (var innerItem in item.Campaigns)
                {
                    lstItems.Add(ObfuscationClass.EncodeId(Convert.ToInt32(innerItem.CampaignId), _appSettings.Prime).ToString());
                }
                advertisements.CampaignId = lstItems;
                advertisements.IntervalId = item.IntervalId;
                advertisements.PromotionsId = promotions.Where(x => x.AdvertisementId == item.AdvertisementId).Select(x => x.PromotionId).FirstOrDefault();
                advertisementsList.Add(advertisements);
            }
            var advertisementsModelList = new List<AdvertisementsGetModel>();
            advertisementsModelList = advertisementsList;
            return advertisementsModelList;
        }

        public List<AdvertisementsGetModelWithCampaign> GetAllAdvertisements(List<Advertisements> advertisements, List<AdvertisementsIntervals> advertisementsCampaignsData, Pagination pageInfo)
        {
            return (from advertisement in advertisements
                    join advertisementsIntervals in advertisementsCampaignsData on advertisement.AdvertisementId equals advertisementsIntervals.AdvertisementId into Details
                    from m in Details.DefaultIfEmpty()
                    select new AdvertisementsGetModelWithCampaign()
                    {
                        AdvertisementId = ObfuscationClass.EncodeId(advertisement.AdvertisementId, _appSettings.Prime).ToString(),
                        CreatedAt = advertisement.CreatedAt,
                        InstitutionId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.InstitutionId), _appSettings.Prime).ToString(),
                        MediaId = ObfuscationClass.EncodeId(Convert.ToInt32(advertisement.MediaId), _appSettings.Prime).ToString(),
                        ResourceName = advertisement.ResourceName,
                        IntervalId = m == null ? null : ObfuscationClass.EncodeId(Convert.ToInt32(m.IntervalId), _appSettings.Prime).ToString(),
                        Campaigns = advertisement.AdvertisementsCampaigns.ToList(),
                    }).AsEnumerable().OrderBy(a => a.AdvertisementId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
        }
    }
}
