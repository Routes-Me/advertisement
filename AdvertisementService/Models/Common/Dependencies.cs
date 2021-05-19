using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.Common
{
    public class Dependencies
    {
        public string IdentifiersUrl { get; set; }
        public string InstitutionUrl { get; set; }
        public string CouponsUrl { get; set; }
        public string PromotionsUrl { get; set; }
        public string PromotionsByAdvertisementUrl { get; set; }
        public string DeleteCouponsUrl { get; set; }
        public string DeleteLinksUrl { get; set; }
        public string GetCouponsUrl { get; set; }
        public string GetLinksUrl { get; set; }
    }
}
