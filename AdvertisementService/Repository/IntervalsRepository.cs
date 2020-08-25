using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class IntervalsRepository : IIntervalsRepository
    {
        private readonly advertisementserviceContext _context;
        public IntervalsRepository(advertisementserviceContext context)
        {
            _context = context;
        }

        public IntervalsResponse DeleteIntervals(int id)
        {
            IntervalsResponse response = new IntervalsResponse();
            try
            {
                var intervalsData = _context.Intervals.Where(x => x.IntervalId == id).FirstOrDefault();
                if (intervalsData == null)
                {
                    response.status = false;
                    response.message = "Interval not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementsIntervals = _context.AdvertisementsIntervals.Where(x => x.IntervalId == id).FirstOrDefault();
                if (intervalsData == null)
                {
                    response.status = false;
                    response.message = "Interval is associated with advertisements.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                _context.Intervals.Remove(intervalsData);
                _context.SaveChanges();
                response.status = true;
                response.message = "Interval deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting interval. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public IntervalsGetResponse GetIntervals(int intervalId, Pagination pageInfo)
        {
            IntervalsGetResponse response = new IntervalsGetResponse();
            int totalCount = 0;
            try
            {
                List<IntervalsModel> intervalsModelList = new List<IntervalsModel>();

                if (intervalId == 0)
                {
                    intervalsModelList = (from interval in _context.Intervals
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = interval.IntervalId,
                                                 Title = interval.Title
                                             }).OrderBy(a => a.IntervalId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Intervals.ToList().Count();
                }
                else
                {
                    intervalsModelList = (from interval in _context.Intervals
                                             where interval.IntervalId == intervalId
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = interval.IntervalId,
                                                 Title = interval.Title
                                             }).OrderBy(a => a.IntervalId).Skip((pageInfo.offset - 1) * pageInfo.limit).Take(pageInfo.limit).ToList();

                    totalCount = _context.Intervals.Where(x => x.IntervalId == intervalId).ToList().Count();
                }

                if (intervalsModelList == null || intervalsModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Interval not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var page = new Pagination
                {
                    offset = pageInfo.offset,
                    limit = pageInfo.limit,
                    total = totalCount
                };

                response.status = true;
                response.message = "Interval data retrived successfully.";
                response.pagination = page;
                response.data = intervalsModelList;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching data. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public IntervalsResponse InsertIntervals(IntervalsModel model)
        {
            IntervalsResponse response = new IntervalsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                Intervals objIntervals = new Intervals()
                {
                    Title = model.Title
                };
                _context.Intervals.Add(objIntervals);
                _context.SaveChanges();

                response.status = true;
                response.message = "Interval inserted successfully.";
                response.responseCode = ResponseCode.Created;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting interval. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public IntervalsResponse UpdateIntervals(IntervalsModel model)
        {
            IntervalsResponse response = new IntervalsResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var intervalData = _context.Intervals.Where(x => x.IntervalId == model.IntervalId).FirstOrDefault();
                if (intervalData == null)
                {
                    response.status = false;
                    response.message = "Interval not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                intervalData.Title = model.Title;
                _context.Intervals.Update(intervalData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Interval updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating interval. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
