using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvertisementService.Repository
{
    public class IntervalsRepository : IIntervalsRepository
    {
        private readonly advertisementserviceContext _context;
        private readonly AppSettings _appSettings;

        public IntervalsRepository(IOptions<AppSettings> appSettings, advertisementserviceContext context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public dynamic DeleteIntervals(string id)
        {
            try
            {
                int intervalIdDecrypted = Obfuscation.Decode(id);
                var intervals = _context.Intervals.Include(x => x.AdvertisementsIntervals).Where(x => x.IntervalId == intervalIdDecrypted).FirstOrDefault();
                if (intervals == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                if (intervals.AdvertisementsIntervals != null)
                    _context.AdvertisementsIntervals.RemoveRange(intervals.AdvertisementsIntervals);

                _context.Intervals.Remove(intervals);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.IntervalDelete, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic GetIntervals(string intervalId, Pagination pageInfo)
        {
            int totalCount = 0;
            try
            {
                IntervalsGetResponse response = new IntervalsGetResponse();
                List<IntervalsModel> intervalsModelList = new List<IntervalsModel>();
                if (string.IsNullOrEmpty(intervalId))
                {
                    intervalsModelList = (from interval in _context.Intervals
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = Obfuscation.Encode(interval.IntervalId),
                                                 Title = interval.Title
                                             }).AsEnumerable().OrderBy(a => a.IntervalId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Intervals.ToList().Count();
                }
                else
                {
                    int intervalIdDecrypted = Obfuscation.Decode(intervalId);
                    intervalsModelList = (from interval in _context.Intervals
                                             where interval.IntervalId == intervalIdDecrypted
                                          select new IntervalsModel()
                                             {
                                              IntervalId = Obfuscation.Encode(interval.IntervalId),
                                              Title = interval.Title
                                             }).AsEnumerable().OrderBy(a => a.IntervalId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();
                    totalCount = _context.Intervals.Where(x => x.IntervalId == intervalIdDecrypted).ToList().Count();
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = CommonMessage.IntervalRetrived;
                response.pagination = page;
                response.data = intervalsModelList;
                response.statusCode = StatusCodes.Status200OK;
                return response;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic InsertIntervals(IntervalsModel model)
        {
            try
            {
                Intervals objIntervals = new Intervals()
                {
                    Title = model.Title
                };
                _context.Intervals.Add(objIntervals);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.IntervalInsert, true);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public dynamic UpdateIntervals(IntervalsModel model)
        {
            try
            {
                int intervalIdDecrypted = Obfuscation.Decode(model.IntervalId);
                var intervalData = _context.Intervals.Where(x => x.IntervalId == intervalIdDecrypted).FirstOrDefault();
                if (intervalData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                intervalData.Title = model.Title;
                _context.Intervals.Update(intervalData);
                _context.SaveChanges();
                return ReturnResponse.SuccessResponse(CommonMessage.IntervalUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}