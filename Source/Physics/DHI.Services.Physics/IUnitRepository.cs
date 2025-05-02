namespace DHI.Services.Physics
{
    public interface IUnitRepository : IRepository<Unit, string>, IDiscreteRepository<Unit, string>, IUpdatableRepository<Unit, string>
    {
    }
}