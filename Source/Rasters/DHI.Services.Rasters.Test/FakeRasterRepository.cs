namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Rasters;

    public class FakeRasterRepository<TImage> : FakeRepository<TImage, DateTime>, IRasterRepository<TImage> where TImage : IRaster
    {
        public FakeRasterRepository(IEnumerable<TImage> images)
            : base(images)
        {
        }

        public DateTime FirstDateTime(ClaimsPrincipal user = null) => _entities.Keys.Min();

        public TImage Last(ClaimsPrincipal user = null) => Get(LastDateTime(user)) | default(TImage);

        public DateTime LastDateTime(ClaimsPrincipal user = null) => _entities.Keys.Max();

        public Dictionary<DateTime, TImage> Get(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _entities.Where(i => i.Key >= from && i.Key <= to).ToDictionary(i => i.Key, i => i.Value);
        }

        public TImage GetFirstAfter(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _entities.First(i => i.Key > dateTime).Value;
        }

        public IEnumerable<TImage> GetFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return dateTimes.Select(dateTime => _entities.OrderBy(r => r.Key).First(i => i.Key > dateTime).Value).ToList();
        }

        public TImage GetLastBefore(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _entities.Last(i => i.Key < dateTime).Value;
        }

        public IEnumerable<TImage> GetLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return dateTimes.Select(dateTime => _entities.OrderBy(r => r.Key).Last(i => i.Key < dateTime).Value).ToList();
        }

        public IEnumerable<DateTime> GetDateTimes(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _entities.OrderBy(r => r.Key).Where(i => i.Key >= from && i.Key <= to).Select(r => r.Key).ToList();
        }

        public IEnumerable<DateTime> GetDateTimesFirstAfter(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return GetFirstAfter(dateTimes).Select(r => r.DateTime).ToList();
        }

        public IEnumerable<DateTime> GetDateTimesLastBefore(IEnumerable<DateTime> dateTimes, ClaimsPrincipal user = null)
        {
            return GetLastBefore(dateTimes).Select(r => r.DateTime).ToList();
        }
    }
}