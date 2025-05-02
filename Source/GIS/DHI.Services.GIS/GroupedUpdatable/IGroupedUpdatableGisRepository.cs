namespace DHI.Services.GIS
{
    public interface IGroupedUpdatableGisRepository<TCollectionId, TFeatureId> : IGroupedGisRepository<TCollectionId>, IUpdatableGisRepository<TCollectionId, TFeatureId>
    {
    }
}