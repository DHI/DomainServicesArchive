namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public abstract class BaseTimeSeriesRepository<TId, TValue> : BaseCoreTimeSeriesRepository<TId, TValue>, ITimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        public virtual Maybe<TimeSeries<TId, TValue>> GetWithValues(TId id, ClaimsPrincipal user = null)
        {
            var timeSeries = Get(id, user).Value;
            if (timeSeries is null)
            {
                return Maybe.Empty<TimeSeries<TId, TValue>>();
            }

            var values = GetValues(id, user).Value;
            return new TimeSeries<TId, TValue>(timeSeries.Id, timeSeries.Name, timeSeries.Group, values)
            {
                DataType = timeSeries.DataType,
                Dimension = timeSeries.Dimension,
                Quantity = timeSeries.Quantity,
                Unit = timeSeries.Unit
            }.ToMaybe();
        }

        public Maybe<TimeSeries<TId, TValue>> GetWithValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var timeSeries = Get(id, user).Value;
            if (timeSeries is null)
            {
                return Maybe.Empty<TimeSeries<TId, TValue>>();
            }

            var values = GetValues(id, from, to, user).Value;
            return new TimeSeries<TId, TValue>(timeSeries.Id, timeSeries.Name, timeSeries.Group, values)
            {
                DataType = timeSeries.DataType,
                Dimension = timeSeries.Dimension,
                Quantity = timeSeries.Quantity,
                Unit = timeSeries.Unit
            }.ToMaybe();
        }

        public virtual IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(IEnumerable<TId> ids, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, TimeSeries<TId, TValue>>();
            foreach (var id in ids)
            {
                var maybe = GetWithValues(id, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }

        public IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(IEnumerable<TId> ids, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, TimeSeries<TId, TValue>>();
            foreach (var id in ids)
            {
                var maybe = GetWithValues(id, from, to, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }

        public virtual bool ContainsDateTime(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var maybe = GetValue(id, dateTime, user);
            return maybe.HasValue;
        }

        public virtual Maybe<SortedSet<DateTime>> GetDateTimes(TId id, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            if (!maybe.HasValue)
            {
                return Maybe.Empty<SortedSet<DateTime>>();
            }

            var dateTimes = maybe.Value.DateTimes;
            return new SortedSet<DateTime>(dateTimes).ToMaybe();
        }

        public virtual Maybe<DateTime> GetFirstDateTime(TId id, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DateTime>() : maybe.Value.GetFirstDateTime();
        }

        public virtual IDictionary<TId, DataPoint<TValue>> GetFirstValue(IEnumerable<TId> ids, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, DataPoint<TValue>>();
            foreach (var id in ids)
            {
                var maybe = GetFirstValue(id, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }

        public virtual Maybe<DataPoint<TValue>> GetFirstValue(TId id, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DataPoint<TValue>>() : maybe.Value.GetFirst();
        }

        public virtual Maybe<DataPoint<TValue>> GetFirstValueAfter(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DataPoint<TValue>>() : maybe.Value.GetFirstAfter(dateTime);
        }

        public virtual Maybe<DateTime> GetLastDateTime(TId id, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DateTime>() : maybe.Value.GetLastDateTime();
        }

        public virtual IDictionary<TId, DataPoint<TValue>> GetLastValue(IEnumerable<TId> ids, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, DataPoint<TValue>>();
            foreach (var id in ids)
            {
                var maybe = GetLastValue(id, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }

        public virtual Maybe<DataPoint<TValue>> GetLastValue(TId id, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DataPoint<TValue>>() : maybe.Value.GetLast();
        }

        public virtual Maybe<DataPoint<TValue>> GetLastValueBefore(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DataPoint<TValue>>() : maybe.Value.GetLastBefore(dateTime);
        }

        public override Maybe<DataPoint<TValue>> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<DataPoint<TValue>>() : maybe.Value.Get(dateTime);
        }

        public abstract Maybe<ITimeSeriesData<TValue>> GetValues(TId id, ClaimsPrincipal user = null);

        public override Maybe<ITimeSeriesData<TValue>> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null) 
        {
            var maybe = GetValues(id, user);
            return !maybe.HasValue ? Maybe.Empty<ITimeSeriesData<TValue>>() : maybe.Value.Get(from, to).ToMaybe();
        }

        public virtual IDictionary<TId, ITimeSeriesData<TValue>> GetValues(IEnumerable<TId> ids, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, ITimeSeriesData<TValue>>();
            foreach (var id in ids)
            {
                var maybe = GetValues(id, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }
    }
}