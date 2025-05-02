namespace DHI.Services.Meshes
{
    public interface IGroupedMeshService<TId> : IMeshService<TId>, IGroupedService<MeshInfo<TId>>
    {
    }

    public interface IGroupedMeshService : IGroupedMeshService<string>
    {
    }
}
