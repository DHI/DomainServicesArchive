namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using DHI.Services.TimeSeries;

    public class FakeTimeSeriesRepository : BaseGroupedUpdatableTimeSeriesRepository<Guid, float>, IGroupedUpdatableRepository
    {
        private readonly Dictionary<Guid, TimeSeries<Guid, float>> _timeSeriesDictionary = new Dictionary<Guid, TimeSeries<Guid, float>>();

        public FakeTimeSeriesRepository(IEnumerable<TimeSeries<Guid, float>> timeSeriesList)
        {
            foreach (var timeSeries in timeSeriesList)
            {
                _timeSeriesDictionary.Add(timeSeries.Id, timeSeries);
            }
        }

        public override void Add(TimeSeries<Guid, float> timeSeries, ClaimsPrincipal user = null)
        {
            _timeSeriesDictionary[timeSeries.Id] = timeSeries;
        }

        public override bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            return _timeSeriesDictionary.Any(e => e.Value.Group == group);
        }

        public override IEnumerable<TimeSeries<Guid, float>> GetAll(ClaimsPrincipal user = null)
        {
            return _timeSeriesDictionary.Values.ToList();
        }

        public override IEnumerable<TimeSeries<Guid, float>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            return _timeSeriesDictionary.Where(t => t.Value.Group == group).Select(t => t.Value).ToList();
        }

        public override IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return _timeSeriesDictionary.Select(t => t.Value.FullName).ToArray();
        }

        public override Maybe<ITimeSeriesData<float>> GetValues(Guid id, ClaimsPrincipal user = null)
        {
            var maybe = Get(id);
            return maybe.HasValue ? maybe.Value.Data.ToMaybe() : Maybe.Empty<ITimeSeriesData<float>>();
        }

        public override void Remove(Guid id, ClaimsPrincipal user = null)
        {
            _timeSeriesDictionary.Remove(id);
        }

        public override void SetValues(Guid id, ITimeSeriesData<float> data, ClaimsPrincipal user = null)
        {
            var timeSeries = Get(id).Value;
            foreach (var dateTime in data.DateTimes)
            {
                timeSeries.Data.DateTimes.Add(dateTime);
            }

            foreach (var value in data.Values)
            {
                timeSeries.Data.Values.Add(value);
            }
        }

        public override void Update(TimeSeries<Guid, float> timeSeries, ClaimsPrincipal user = null)
        {
            _timeSeriesDictionary[timeSeries.Id] = timeSeries;
        }

        public override void RemoveValues(Guid id, ClaimsPrincipal user = null)
        {
            var timeSeries = Get(id).Value;
            timeSeries.Data.DateTimes.Clear();
            timeSeries.Data.Values.Clear();
        }

        public override void RemoveValues(Guid id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var timeSeries = Get(id).Value;
            var indices = timeSeries.Data.DateTimes.Select((dateTime, i) => new {dateTime, i}).Where(x => x.dateTime > from && x.dateTime < to ).Select(x => x.i).ToArray();
            foreach (var index in indices.OrderByDescending(i => i))
            {
                timeSeries.Data.DateTimes.RemoveAt(index);
                timeSeries.Data.Values.RemoveAt(index);
            }
        }

        public void RemoveByGroup(string group, ClaimsPrincipal user = null)
        {
            var timeSeriesList = GetByGroup(group).ToArray();
            foreach (var timeSeries in timeSeriesList)
            {
                _timeSeriesDictionary.Remove(timeSeries.Id);
            }
        }
    }
}