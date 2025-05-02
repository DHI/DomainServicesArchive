namespace DHI.Services.Scalars
{
    public interface IGroupedScalarService<TId, TFlag> : IScalarService<TId, TFlag>, IGroupedService<Scalar<TId, TFlag>> where TFlag : struct
    {
    }
}