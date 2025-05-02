namespace DHI.Services.TimeSteps
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface ITimeStepService<TItemId, TData> where TData : class
    {
        DateTime? GetFirstDateTime(ClaimsPrincipal user = null);

        DateTime? GetLastDateTime(ClaimsPrincipal user = null);

        DateTime[] GetDateTimes(ClaimsPrincipal user = null);

        TItemId[] GetItemIds(ClaimsPrincipal user = null);

        Item<TItemId>[] GetItems(ClaimsPrincipal user = null);

        TData Get(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);

        IDictionary<TItemId, IDictionary<DateTime, TData>> Get(IDictionary<TItemId, IEnumerable<DateTime>> ids, ClaimsPrincipal user = null);

        TData GetFirst(TItemId itemId, ClaimsPrincipal user = null);

        TData GetLast(TItemId itemId, ClaimsPrincipal user = null);

        TData GetFirstAfter(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);

        TData GetLastBefore(TItemId itemId, DateTime dateTime, ClaimsPrincipal user = null);
    }
}