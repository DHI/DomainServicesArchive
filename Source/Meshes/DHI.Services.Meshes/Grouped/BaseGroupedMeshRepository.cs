using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Meshes.Test")]

namespace DHI.Services.Meshes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     Abstract base class for a grouped mesh repository.
    ///     Implements the <see cref="IGroupedMeshRepository{TId}" /> interface
    /// </summary>
    /// <typeparam name="TId">The type of the mesh identifier.</typeparam>
    /// <seealso cref="IGroupedMeshRepository{TId}" />
    public abstract class BaseGroupedMeshRepository<TId> : BaseMeshRepository<TId>, IGroupedMeshRepository<TId>
    {
        /// <inheritdoc />
        public abstract bool ContainsGroup(string group, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public abstract IEnumerable<MeshInfo<TId>> GetByGroup(string group, ClaimsPrincipal? user = null);

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal? user = null)
        {
            return GetByGroup(group, user).Select(m => m.FullName).ToArray();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal? user = null)
        {
            return GetAll(user).Select(m => m.FullName).ToArray();
        }
    }
}