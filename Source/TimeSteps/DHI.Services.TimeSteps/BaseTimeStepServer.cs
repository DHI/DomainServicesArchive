namespace DHI.Services.TimeSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseTimeStepServer<TItemId, TData> : ITimeStepServer<TItemId, TData> where TData : class
    {
        public abstract IList<DateTime> GetDateTimes(ClaimsPrincipal user = null);

        public virtual DateTime? GetFirstDateTime(ClaimsPrincipal user = null) => GetDateTimes(user).Any() ? GetDateTimes(user).First() : null;

        public virtual IEnumerable<TItemId> GetItemIds(ClaimsPrincipal user = null)
        {
            return GetItems(user).Select(item => item.Id).ToArray();
        }

        public abstract IEnumerable<Item<TItemId>> GetItems(ClaimsPrincipal user = null);

        public virtual DateTime? GetLastDateTime(ClaimsPrincipal user = null) => GetDateTimes(user).Any() ? GetDateTimes(user).Last() : null;

        public virtual bool ContainsDateTime(DateTime dateTime, ClaimsPrincipal user = null)
        {
            return GetDateTimes(user).Contains(dateTime);
        }

        public virtual bool ContainsItem(TItemId itemId, ClaimsPrincipal user = null)
        {
            return GetItemIds(user).Contains(itemId);
        }

        public abstract Maybe<TData> Get(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);

        public virtual IDictionary<TItemId, IDictionary<DateTime, TData>> Get(IDictionary<TItemId, IEnumerable<DateTime>> ids, ClaimsPrincipal user = null)
        {
            var result = new Dictionary<TItemId, IDictionary<DateTime, TData>>();
            foreach (var id in ids)
            {
                var dictionary = new Dictionary<DateTime, TData>();
                foreach (var dateTime in id.Value)
                {
                    var maybe = Get(id.Key, dateTime, user);
                    if (maybe.HasValue)
                    {
                        dictionary.Add(dateTime, maybe.Value);
                    }
                }

                result.Add(id.Key, dictionary);
            }

            return result;
        }

        public virtual Maybe<TData> GetFirstAfter(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return Get(itemId, GetDateTimes(user).First(d => d > dateTime), user);
        }

        public virtual Maybe<TData> GetLastBefore(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return Get(itemId, GetDateTimes(user).Last(d => d < dateTime), user);
        }
    }
}