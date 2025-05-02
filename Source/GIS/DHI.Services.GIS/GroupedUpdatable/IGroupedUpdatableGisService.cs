namespace DHI.Services.GIS
{
    public interface IGroupedUpdatableGisService<TCollectionId, TFeatureId> : IGroupedGisService<TCollectionId>, IUpdatableGisService<TCollectionId, TFeatureId>
    {
    }
}