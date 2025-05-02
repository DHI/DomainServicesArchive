namespace DHI.Services.Meshes
{
    /// <summary>
    ///     Interface IGroupedMeshRepository
    /// </summary>
    /// <typeparam name="TId">The type of the mesh identifier.</typeparam>
    public interface IGroupedMeshRepository<TId> : IMeshRepository<TId>, IGroupedRepository<MeshInfo<TId>>
    {
    }
}