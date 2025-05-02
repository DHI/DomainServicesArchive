namespace DHI.Services.Scalars
{
    using System.Security.Claims;

    public interface IScalarRepository<TId, TFlag> : 
        IRepository<Scalar<TId, TFlag>, TId>,
        IDiscreteRepository<Scalar<TId, TFlag>, TId>,
        IUpdatableRepository<Scalar<TId, TFlag>, TId> where TFlag : struct
    {
        void SetData(TId id, ScalarData<TFlag> data, ClaimsPrincipal user = null);

        void SetLocked(TId id, bool locked, ClaimsPrincipal user = null);
    }
}