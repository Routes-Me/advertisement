using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using AdvertisementService.Internal.Abstraction;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Helper.Models;
using AdvertisementService.Internal.Dto;
using System.Data;

namespace AdvertisementService.Internal.Repository
{
    public class AdvertisementsReportRepository : IAdvertisementsReportRepository
    {
        private readonly AdvertisementContext _context;

        public AdvertisementsReportRepository(IOptions<AppSettings> appSettings, AdvertisementContext context)
        {
            _context = context;
        }

        public List<AdvertisementReportDto> ReportAdvertisements(List<int> advertisementIds, List<string> attributes)
        {
            return _context.Advertisements
                .Where(v => advertisementIds.Contains(v.AdvertisementId))
                .Select(v => new AdvertisementReportDto {
                    AdvertisementId = v.AdvertisementId,
                    Name = attributes.Contains(nameof(v.Name)) ? v.Name : null,
                    ResourceNumber = attributes.Contains(nameof(v.ResourceNumber)) ? v.ResourceNumber : null,
                    TintColor = attributes.Contains(nameof(v.TintColor)) ? v.TintColor : null,
                    InvertedTintColor = attributes.Contains(nameof(v.InvertedTintColor)) ? v.InvertedTintColor : null,
                    CreatedAt = v.CreatedAt
                }).ToList();
        }
    }
}
