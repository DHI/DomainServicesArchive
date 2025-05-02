namespace DHI.Services.TimeSteps
{
    using System;
    using System.Collections.Generic;

    public class TimeStep<TItemId, TData> : BaseEntity<string>, IComparable<TimeStep<TItemId, TData>>, IEquatable<TimeStep<TItemId, TData>>
    {
        public TimeStep(TItemId itemId, DateTime dateTime, TData data)
            : base(itemId.ToString() + dateTime)
        {
            ItemId = itemId;
            DateTime = dateTime;
            Data = data;
        }

        public TItemId ItemId { get; }

        public DateTime DateTime { get; }

        public TData Data { get; }

        public int CompareTo(TimeStep<TItemId, TData> other)
        {
            var result = DateTime.CompareTo(other.DateTime);
            if (result != 0)
            {
                return result;
            }

            return Comparer<TItemId>.Default.Compare(ItemId, other.ItemId);
        }

        public bool Equals(TimeStep<TItemId, TData> other)
        {
            return DateTime.Equals(other.DateTime) && EqualityComparer<TItemId>.Default.Equals(ItemId, other.ItemId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TimeStep<TItemId, TData> step && Equals(step);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DateTime.GetHashCode() * 397) ^ EqualityComparer<TItemId>.Default.GetHashCode(ItemId);
            }
        }
    }
}