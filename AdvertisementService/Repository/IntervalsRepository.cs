using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvertisementService.Repository
{
    public class IntervalsRepository : IIntervalsRepository
    {
        private readonly advertisementserviceContext _context;
        public IntervalsRepository(advertisementserviceContext context)
        {
            _context = context;
        }

        public dynamic DeleteIntervals(string id)
        {
            try
            {
                var intervals = _context.Intervals.Include(x => x.AdvertisementsIntervals).Where(x => x.IntervalId == Convert.ToInt32(id)).FirstOrDefault();
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
                if (intervalId == "0")
                {
                    intervalsModelList = (from interval in _context.Intervals
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = interval.IntervalId.ToString(),
                                                 Title = interval.Title
                                             }).OrderBy(a => a.IntervalId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Intervals.ToList().Count();
                }
                else
                {
                    intervalsModelList = (from interval in _context.Intervals
                                             where interval.IntervalId == Convert.ToInt32(intervalId)
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = interval.IntervalId.ToString(),
                                                 Title = interval.Title
                                             }).OrderBy(a => a.IntervalId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Intervals.Where(x => x.IntervalId == Convert.ToInt32(intervalId)).ToList().Count();
                }

                if (intervalsModelList == null || intervalsModelList.Count == 0)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

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
                var intervalData = _context.Intervals.Where(x => x.IntervalId == Convert.ToInt32(model.IntervalId)).FirstOrDefault();
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