namespace DHI.Services.Scalars
{
    public interface IGroupedScalarRepository<TId, TFlag> : IScalarRepository<TId, TFlag>, IGroupedRepository<Scalar<TId, TFlag>> where TFlag : struct
    {
    }
}