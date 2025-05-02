namespace DHI.Services.Scalars
{
    using System.Security.Claims;

    public interface IScalarService<TId, TFlag> :
        IService<Scalar<TId, TFlag>, TId>,
        IDiscreteService<Scalar<TId, TFlag>, TId>,
        IUpdatableService<Scalar<TId, TFlag>, TId> where TFlag : struct
    {
        void SetData(TId id, ScalarData<TFlag> data, bool log = true, ClaimsPrincipal user = null);

        bool TrySetDataOrAdd(Scalar<TId, TFlag> scalar, bool log = true, ClaimsPrincipal user = null);

        void SetLocked(TId id, bool locked, ClaimsPrincipal user = null);
    }
}