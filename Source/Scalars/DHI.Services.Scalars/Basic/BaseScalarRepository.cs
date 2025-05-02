namespace DHI.Services.Scalars
{
    using System.Security.Claims;

    public abstract class BaseScalarRepository<TId, TFlag> : BaseDiscreteRepository<Scalar<TId, TFlag>, TId>, IScalarRepository<TId, TFlag> where TFlag : struct
    {
        public abstract void Add(Scalar<TId, TFlag> entity, ClaimsPrincipal user = null);

        public abstract void Remove(TId id, ClaimsPrincipal user = null);

        public abstract void Update(Scalar<TId, TFlag> entity, ClaimsPrincipal user = null);

        public virtual void SetData(TId id, ScalarData<TFlag> data, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.SetData(data);
            Update(scalar);
        }

        public virtual void SetLocked(TId id, bool locked, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.Locked = locked;
            Update(scalar);
        }
    }
}