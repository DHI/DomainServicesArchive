namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;


    /// <summary>
    ///     Immutable Json Repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    /// <seealso cref="IRepository{TEntity, TEntityId}" />
    /// <seealso cref="IDiscreteRepository{TEntity, TEntityId}" />
    /// <seealso cref="IImmutableRepository{TEntity, TEntityId}" />
    public class ImmutableJsonRepository<TEntity, TEntityId> :
        BaseJsonRepositoryDefault<TEntity, TEntityId>,
        IImmutableRepository<TEntity, TEntityId> where TEntity : IEntity<TEntityId>
    {
        private static readonly object _syncObject = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableJsonRepository{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public ImmutableJsonRepository(string filePath)
            : this(filePath, null, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableJsonRepository{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for <typeparamref name="TEntity"/></param>
        public ImmutableJsonRepository(string filePath,
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
        /// <param name="comparer">Equality comparer</param>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public ImmutableJsonRepository(string filePath,
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
        public override int Count(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                return Entities.Count;
            }
        }

        /// <summary>
        ///     Determines whether an entity with the specified identifier exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if entity with the specified identifier exists, <c>false</c> otherwise.</returns>
        public override bool Contains(TEntityId id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                return Entities.ContainsKey(id);
            }
        }

        /// <summary>
        ///     Gets all entities.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public override IEnumerable<TEntity> GetAll(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                return InternalEntities.Values;
            }
        }

        /// <summary>
        ///     Gets all entity identifiers.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntityId&gt;.</returns>
        public override IEnumerable<TEntityId> GetIds(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                return Entities.Values.Select(e => e.Id);
            }
        }

        /// <summary>
        ///     Gets the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>Maybe&lt;TEntity&gt;.</returns>
        public override Maybe<TEntity> Get(TEntityId id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return Maybe.Empty<TEntity>();
                }

                InternalEntities.TryGetValue(id, out var entity);
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
        public virtual void Remove(TEntityId id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                if (Entities.Remove(id))
                {
                    Serialize();
                }
            }
        }

        /// <summary>
        ///     Gets the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return new List<TEntity>();
                }

                return InternalEntities.Values.AsQueryable().Where(predicate);
            }
        }

        /// <summary>
        ///     Removes the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        public virtual void Remove(Expression<Func<TEntity, bool>> predicate, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var toRemove = Get(predicate);
                foreach (var entity in toRemove)
                {
                    Entities.Remove(entity.Id);
                }

                Serialize();
            }
        }
    }
}