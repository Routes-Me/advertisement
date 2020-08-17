using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.ResponseModel
{
    public class MediasModel
    {
        public int MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaType { get; set; }
        public int? MediaMetadataId { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; }
    }
}
