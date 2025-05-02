namespace DHI.Services.TimeSteps
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface ITimeStepServer<TItemId, TData> where TData : class
    {
        IList<DateTime> GetDateTimes(ClaimsPrincipal user = null);

        DateTime? GetFirstDateTime(ClaimsPrincipal user = null);

        IEnumerable<Item<TItemId>> GetItems(ClaimsPrincipal user = null);

        IEnumerable<TItemId> GetItemIds(ClaimsPrincipal user = null);

        DateTime? GetLastDateTime(ClaimsPrincipal user = null);

        bool ContainsDateTime(DateTime dateTime, ClaimsPrincipal user = null);

        bool ContainsItem(TItemId itemId, ClaimsPrincipal user = null);

        Maybe<TData> Get(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);

        IDictionary<TItemId, IDictionary<DateTime, TData>> Get(IDictionary<TItemId, IEnumerable<DateTime>> ids, ClaimsPrincipal user = null); 

        Maybe<TData> GetFirstAfter(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);

        Maybe<TData> GetLastBefore(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);
    }
}