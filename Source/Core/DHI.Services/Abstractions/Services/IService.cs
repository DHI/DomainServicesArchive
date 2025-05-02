#nullable enable
namespace DHI.Services
{
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Interface IService
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    public interface IService<TEntity, in TEntityId> where TEntity : IEntity<TEntityId>
    {
        /// <summary>
        ///     Gets the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>TEntity.</returns>
        TEntity Get(TEntityId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Trys to get the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if entity was found, false otherwise</returns>
        bool TryGet(TEntityId id, out TEntity entity, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets a list of entities with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        IEnumerable<TEntity> Get(IEnumerable<TEntityId> ids, ClaimsPrincipal user = null);

        /// <summary>
        ///     Trys to get a list of entities with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if all entities were found, false otherwise.</returns>
        bool TryGet(IEnumerable<TEntityId> ids, out IEnumerable<TEntity?> entities, ClaimsPrincipal user = null);
    }
}