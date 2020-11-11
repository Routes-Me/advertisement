using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.ResponseModel
{
    public class PromotionsModel
    {
        public string PromotionId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string LogoUrl { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
    }

    public class PromotionsGetModel
    {
        public string PromotionId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int? UsageLimit { get; set; }
        public string AdvertisementId { get; set; }
        public string InstitutionId { get; set; }
        public bool? IsSharable { get; set; }
        public string LogoUrl { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
    }

    public class PromotionsModelForContent
    {
        public string PromotionId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string LogoUrl { get; set; }
        public string Code { get; set; }
        public string Link { get; set; }
    }

}
