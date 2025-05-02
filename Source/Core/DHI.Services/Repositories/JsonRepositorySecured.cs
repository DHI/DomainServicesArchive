namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Authorization;

    /// <summary>
    ///     Generic Json Repository supporting permissions on entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    /// <seealso cref="DHI.Services.IRepository{TEntity, TEntityId}" />
    /// <seealso cref="DHI.Services.IDiscreteRepository{TEntity, TEntityId}" />
    /// <seealso cref="DHI.Services.IUpdatableRepository{TEntity, TEntityId}" />
    public class JsonRepositorySecured<TEntity, TEntityId> :
        BaseJsonRepositoryDefault<TEntity, TEntityId>,
        IUpdatableRepository<TEntity, TEntityId> where TEntity : ISecuredEntity<TEntityId>
    {
        private static readonly object _syncObject = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonRepository{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public JsonRepositorySecured(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonRepositorySecured{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for <typeparamref name="TEntity"/></param>
        public JsonRepositorySecured(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<TEntityId> comparer = null)
            : base(filePath, converters, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableJsonRepository{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer">Equality comparer for <typeparamref name="TEntity"/></param>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public JsonRepositorySecured(string filePath,
            JsonSerializerOptions serializerOptions = null,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<TEntityId> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
        }

        /// <summary>
        ///     The total number of entities.
        /// </summary>
        /// <param name="user">The user.</param>
        public override int Count(ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                return Entities.Count(e => e.Value.IsAllowed(user, "read"));
            }
        }

        /// <summary>
        ///     Determines whether an entity with the specified identifier exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if entity with the specified identifier exists, <c>false</c> otherwise.</returns>
        public override bool Contains(TEntityId id, ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                return Entities.Where(e => e.Value.IsAllowed(user, "read")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value, Comparer).ContainsKey(id);
            }
        }

        /// <summary>
        ///     Gets all entities.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public override IEnumerable<TEntity> GetAll(ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                Deserialize();
                return Entities.Where(e => e.Value.IsAllowed(user, "read")).Select(kvp => kvp.Value);
            }
        }

        /// <summary>
        ///     Gets all entity identifiers.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntityId&gt;.</returns>
        public override IEnumerable<TEntityId> GetIds(ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                return Entities.Where(e => e.Value.IsAllowed(user, "read")).Select(kvp => kvp.Value.Id);
            }
        }

        /// <summary>
        ///     Gets the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>Maybe&lt;TEntity&gt;.</returns>
        public override Maybe<TEntity> Get(TEntityId id, ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return Maybe.Empty<TEntity>();
                }

                InternalEntities.Where(e => e.Value.IsAllowed(user, "read")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value, Comparer).TryGetValue(id, out var entity);
                return entity == null || entity.Equals(default(TEntity)) ? Maybe.Empty<TEntity>() : entity.ToMaybe();
            }
        }

        /// <summary>
        ///     Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="user">The user.</param>
        public virtual void Add(TEntity entity, ClaimsPrincipal user = null)
        {
            if (!entity.Permissions.Any())
            {
                throw new ArgumentException("No permissions were defined for the entity.", nameof(entity));
            }

            if (entity.Permissions.All(p => p.Operation != "read"))
            {
                throw new ArgumentException("No 'read' permissions were defined for the entity.", nameof(entity));
            }

            lock (_syncObject)
            {
                Entities.Add(entity.Id, entity);
                Serialize();
            }
        }

        /// <summary>
        ///     Removes the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        public virtual void Remove(TEntityId id, ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                if (!Entities[id].IsAllowed(user, "delete"))
                {
                    throw new ArgumentException($"User '{user.GetUserId()}' does not have permission to remove entity with id '{id}'.", nameof(id));
                }

                if (Entities.Remove(id))
                {
                    Serialize();
                }
            }
        }

        /// <summary>
        ///     Updates the specified entity.
        /// </summary>
        /// <param name="updatedEntity">The updated entity.</param>
        /// <param name="user">The user.</param>
        public virtual void Update(TEntity updatedEntity, ClaimsPrincipal user)
        {
            if (!updatedEntity.Permissions.Any())
            {
                throw new ArgumentException("No permissions were defined for the entity.", nameof(updatedEntity));
            }

            if (updatedEntity.Permissions.All(p => p.Operation != "read"))
            {
                throw new ArgumentException("No 'read' permissions were defined for the entity.", nameof(updatedEntity));
            }

            lock (_syncObject)
            {
                if (!Entities[updatedEntity.Id].IsAllowed(user, "update"))
                {
                    throw new ArgumentException($"User '{user.GetUserId()}' does not have permission to update entity with id '{updatedEntity.Id}'.", nameof(updatedEntity));
                }

                if (updatedEntity is ITraceableEntity<TEntityId> entity)
                {
                    entity.Added = InternalEntities[entity.Id].Added;
                }

                Entities[updatedEntity.Id] = updatedEntity;
                Serialize();
            }
        }

        /// <summary>
        ///     Gets the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return new List<TEntity>();
                }

                return InternalEntities.Values.AsQueryable().Where(predicate).Where(e => e.IsAllowed(user, "read"));
            }
        }

        /// <summary>
        ///     Removes the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        public virtual void Remove(Expression<Func<TEntity, bool>> predicate, ClaimsPrincipal user)
        {
            lock (_syncObject)
            {
                var toRemove = Get(predicate, user);
                foreach (var entity in toRemove)
                {
                    Entities.Remove(entity.Id);
                }

                Serialize();
            }
        }
    }
}