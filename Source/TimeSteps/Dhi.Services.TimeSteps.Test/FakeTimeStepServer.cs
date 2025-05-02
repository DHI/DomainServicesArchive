namespace DHI.Services.TimeSteps.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using TimeSteps;

    internal struct TimeStep<TItemId> : IComparable<TimeStep<TItemId>>, IEquatable<TimeStep<TItemId>>
    {
        public TimeStep(TItemId itemId, DateTime dateTime)
            : this()
        {
            ItemId = itemId;
            DateTime = dateTime;
        }

        public DateTime DateTime { get; }

        public TItemId ItemId { get; }

        public int CompareTo(TimeStep<TItemId> other)
        {
            var result = DateTime.CompareTo(other.DateTime);
            if (result != 0)
            {
                return result;
            }

            return Comparer<TItemId>.Default.Compare(ItemId, other.ItemId);
        }

        public bool Equals(TimeStep<TItemId> other)
        {
            return DateTime.Equals(other.DateTime) && EqualityComparer<TItemId>.Default.Equals(ItemId, other.ItemId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TimeStep<TItemId> step && Equals(step);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DateTime.GetHashCode() * 397) ^ EqualityComparer<TItemId>.Default.GetHashCode(ItemId);
            }
        }
    }

    internal class FakeTimeStepServer<TItemId, TData> : BaseTimeStepServer<TItemId, TData> where TData : class
    {
        private readonly SortedDictionary<TimeStep<TItemId>, TData> _data;

        public FakeTimeStepServer(IDictionary<TimeStep<TItemId>, TData> data)
        {
            _data = new SortedDictionary<TimeStep<TItemId>, TData>(data);
        }

        public override IList<DateTime> GetDateTimes(ClaimsPrincipal user = null)
        {
            return _data.Keys.Select(timeStep => timeStep.DateTime).Distinct().ToArray();
        }

        public override IEnumerable<Item<TItemId>> GetItems(ClaimsPrincipal user = null)
        {
            var items = new List<Item<TItemId>>();
            foreach (var timeStepId in _data.Keys.Select(timeStep => timeStep.ItemId).Distinct())
            {
                items.Add(new Item<TItemId>(timeStepId, timeStepId.ToString()));
            }

            return items;
        }

        public override Maybe<TData> Get(TItemId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var timeStep = new TimeStep<TItemId>(id, dateTime);
            return _data.ContainsKey(timeStep) ? _data.Single(pair => pair.Key.Equals(new TimeStep<TItemId>(id, dateTime))).Value.ToMaybe() : Maybe.Empty<TData>();
        }
    }
}