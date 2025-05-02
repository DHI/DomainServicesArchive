namespace DHI.Services.Places
{
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IPlaceRepository<TCollectionId> :
        IRepository<Place<TCollectionId>, string>,
        IDiscreteRepository<Place<TCollectionId>, string>,
        IUpdatableRepository<Place<TCollectionId>, string>,
        IGroupedRepository<Place<TCollectionId>> where TCollectionId : notnull
    {

        Maybe<Indicator> GetIndicator(string placeId, string type, ClaimsPrincipal? user = null);

        IDictionary<string, IDictionary<string, Indicator>> GetIndicators(ClaimsPrincipal? user = null);

        IDictionary<string, Indicator> GetIndicatorsByPlace(string placeId, ClaimsPrincipal? user = null);

        IDictionary<string, Indicator> GetIndicatorsByType(string type, ClaimsPrincipal? user = null);

        IDictionary<string, Indicator> GetIndicatorsByGroupAndType(string group, string type, ClaimsPrincipal? user = null);

        IDictionary<string, IDictionary<string, Indicator>> GetIndicatorsByGroup(string group, ClaimsPrincipal? user = null);
    }
}