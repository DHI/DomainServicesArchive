namespace DHI.Services.TimeSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class TimeStepService<TItemId, TData> : ITimeStepService<TItemId, TData> where TData : class
    {
        private readonly ITimeStepServer<TItemId, TData> _timeStepServer;

        public TimeStepService(ITimeStepServer<TItemId, TData> timeStepServer)
        {
            _timeStepServer = timeStepServer ?? throw new ArgumentNullException(nameof(timeStepServer));
        }

        public TData Get(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null)
        {
            if (!_timeStepServer.ContainsDateTime(dateTime, user))
            {
                throw new KeyNotFoundException($"Time step server does not contain DateTime '{dateTime}'.");
            }

            var maybe = _timeStepServer.Get(itemId, dateTime, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The item with ID '{itemId}' was not found.");
            }

            return maybe.Value;
        }

        public IDictionary<TItemId, IDictionary<DateTime, TData>> Get(IDictionary<TItemId, IEnumerable<DateTime>> ids, ClaimsPrincipal user = null)
        {
            return _timeStepServer.Get(ids, user);
        }

        public TData GetFirst(TItemId itemId, ClaimsPrincipal user = null)
        {
            var firstDateTime = GetFirstDateTime(user);
            return firstDateTime == null ? null : _timeStepServer.Get(itemId, (DateTime)firstDateTime, user) | default(TData);
        }

        public TData GetLast(TItemId itemId, ClaimsPrincipal user = null)
        {
            var lastDateTime = GetLastDateTime(user);
            return lastDateTime == null ? null : _timeStepServer.Get(itemId, (DateTime)lastDateTime, user) | default(TData);
        }

        public DateTime[] GetDateTimes(ClaimsPrincipal user = null)
        {
            return _timeStepServer.GetDateTimes(user).ToArray();
        }

        public TItemId[] GetItemIds(ClaimsPrincipal user = null)
        {
            return _timeStepServer.GetItemIds(user).ToArray();
        }

        public Item<TItemId>[] GetItems(ClaimsPrincipal user = null)
        {
            return _timeStepServer.GetItems(user).ToArray();
        }

        public TData GetFirstAfter(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var lastDateTime = _timeStepServer.GetLastDateTime(user);
            if (lastDateTime == null)
            {
                return null;
            }

            if (dateTime >= lastDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime,
                    $"DateTime value is out of range. Last DateTime is '{lastDateTime}'.");
            }

            return _timeStepServer.GetFirstAfter(itemId, dateTime, user) | default(TData);
        }

        public TData GetLastBefore(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var firstDateTime = _timeStepServer.GetFirstDateTime(user);
            if (firstDateTime == null)
            {
                return null;
            }

            if (dateTime <= firstDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime,
                    $"DateTime value is out of range. First DateTime is '{firstDateTime}'");
            }

            return _timeStepServer.GetLastBefore(itemId, dateTime, user) | default(TData);
        }

        public DateTime? GetFirstDateTime(ClaimsPrincipal user = null)
        {
            return _timeStepServer.GetFirstDateTime(user);
        }

        public DateTime? GetLastDateTime(ClaimsPrincipal user = null)
        {
            return _timeStepServer.GetLastDateTime(user);
        }

        public static Type[] GetServerTypes(string path = null)
        {
            return Service.GetProviderTypes<ITimeStepServer<TItemId, TData>>(path);
        }
    }
}