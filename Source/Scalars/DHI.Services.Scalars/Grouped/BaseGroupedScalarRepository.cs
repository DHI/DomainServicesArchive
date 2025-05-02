namespace DHI.Services.Scalars
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseGroupedScalarRepository<TId, TFlag> : BaseScalarRepository<TId, TFlag>, IGroupedScalarRepository<TId, TFlag> where TFlag : struct
    {
        public abstract bool ContainsGroup(string group, ClaimsPrincipal user = null);

        public abstract IEnumerable<Scalar<TId, TFlag>> GetByGroup(string group, ClaimsPrincipal user = null);

        public IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(s => s.FullName).ToArray();
        }

        public IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(s => s.FullName).ToArray();
        }
    }
}