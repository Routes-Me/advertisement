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
                    response.intervalsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementsIntervals = _context.Advertisementsintervals.Where(x => x.IntervalId == id).FirstOrDefault();
                if (intervalsData == null)
                {
                    response.status = false;
                    response.message = "Interval is associated with advertisements.";
                    response.intervalsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                _context.Intervals.Remove(intervalsData);
                _context.SaveChanges();
                response.status = true;
                response.message = "Interval deleted successfully.";
                response.intervalsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting interval. Error Message - " + ex.Message;
                response.intervalsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public IntervalsResponse GetIntervals(GetIntervalsModel model)
        {
            IntervalsResponse Response = new IntervalsResponse();
            IntervalsDetails objIntervalsDetails = new IntervalsDetails();
            int totalCount = 0;
            try
            {
                List<IntervalsModel> objIntervalsModelList = new List<IntervalsModel>();
                IntervalsDetailsData objIntervalsDetailsData = new IntervalsDetailsData();

                if (model.IntervalId == 0)
                {
                    objIntervalsModelList = (from interval in _context.Intervals
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = interval.IntervalId,
                                                 Title = interval.Title
                                             }).OrderBy(a => a.IntervalId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                    totalCount = (from interval in _context.Intervals
                                  select new IntervalsModel()
                                  {
                                      IntervalId = interval.IntervalId,
                                      Title = interval.Title
                                  }).ToList().Count();
                }
                else
                {
                    objIntervalsModelList = (from interval in _context.Intervals
                                             where interval.IntervalId == model.IntervalId
                                             select new IntervalsModel()
                                             {
                                                 IntervalId = interval.IntervalId,
                                                 Title = interval.Title
                                             }).OrderBy(a => a.IntervalId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                    totalCount = (from interval in _context.Intervals
                                  where interval.IntervalId == model.IntervalId
                                  select new IntervalsModel()
                                  {
                                      IntervalId = interval.IntervalId,
                                      Title = interval.Title
                                  }).ToList().Count();
                }

                if (objIntervalsModelList == null || objIntervalsModelList.Count == 0)
                {
                    Response.status = false;
                    Response.message = "Interval not found.";
                    Response.intervalsDetails = null;
                    Response.responseCode = ResponseCode.NotFound;
                    return Response;
                }

                var page = new Pagination
                {
                    TotalCount = totalCount,
                    CurrentPage = model.currentPage,
                    PageSize = model.pageSize,
                    TotalPages = (int)Math.Ceiling(decimal.Divide(totalCount, model.pageSize)),
                    IndexOne = ((model.currentPage - 1) * model.pageSize + 1),
                    IndexTwo = (((model.currentPage - 1) * model.pageSize + model.pageSize) <= totalCount ? ((model.currentPage - 1) * model.pageSize + model.pageSize) : totalCount)
                };

                objIntervalsDetailsData.intervals = objIntervalsModelList;
                objIntervalsDetails.data = objIntervalsDetailsData;
                objIntervalsDetails.pagination = page;

                Response.message = "Interval data retrived successfully.";
                Response.status = true;
                Response.intervalsDetails = objIntervalsDetails;
                Response.responseCode = ResponseCode.Success;
                return Response;
            }
            catch (Exception ex)
            {
                Response.status = false;
                Response.message = "Something went wrong while fetching data. Error Message - " + ex.Message;
                Response.intervalsDetails = null;
                Response.responseCode = ResponseCode.InternalServerError;
                return Response;
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
                    response.intervalsDetails = null;
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
                response.intervalsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting interval. Error Message - " + ex.Message;
                response.intervalsDetails = null;
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
                    response.intervalsDetails = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var intervalData = _context.Intervals.Where(x => x.IntervalId == model.IntervalId).FirstOrDefault();
                if (intervalData == null)
                {
                    response.status = false;
                    response.message = "Interval not found.";
                    response.intervalsDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                intervalData.Title = model.Title;
                _context.Intervals.Update(intervalData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Interval updated successfully.";
                response.intervalsDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating interval. Error Message - " + ex.Message;
                response.intervalsDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
