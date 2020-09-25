using Microsoft.AspNetCore.Http;
using System;

namespace AdvertisementService.Models.ResponseModel
{
    public class MediasModel
    {
        public string MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaType { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; }
        public IFormFile media { get; set; }
    }

    public class GetMediasModel
    {
        public string MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaType { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; } = 0;
    }
} 