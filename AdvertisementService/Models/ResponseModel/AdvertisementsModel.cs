using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.ResponseModel
{
    public class AdvertisementsModel
    {
        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
    }

    public class PostAdvertisementsModel
    {
        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public int? IntervalId { get; set; }
        public int? CampaignId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
    }

    public class GetActiveCampAdModel
    {
        public int ContentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class GetActiveCampAdWithQRCodeModel
    {
        public int ContentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public QRCodeModel qrCode { get; set; }
    }

    public class QRCodeModel
    {
        public string Details { get; set; }
        public string Url { get; set; }
    }
}
