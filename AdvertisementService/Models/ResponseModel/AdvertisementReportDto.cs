using System;

namespace AdvertisementService.Internal.Dto
{
    public class AdvertisementReportDto
    {
        public int AdvertisementId { get; set; }
        public string ResourceNumber { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
    }
}
