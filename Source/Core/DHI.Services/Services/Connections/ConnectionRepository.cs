namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    /// <summary>
    ///     JSON Connection Repository.
    /// </summary>
    /// <seealso cref="JsonRepository{IConnection, String}" />
    /// <seealso cref="IConnectionRepository" />
    public class ConnectionRepository : JsonRepository<IConnection, string>, IConnectionRepository
    {
        private static readonly object _syncObject = new();
        private readonly JsonConverter[] _requiredConverters = new JsonConverter[]
        {
            new ConnectionDictionaryConverter(),
            new ConnectionConverter(),
            new TypeResolverConverter<Parameters>(),
            new TypeResolverConverter<ConnectionType>(),
            new TypeResolverConverter<ProviderArgument>(),
            new TypeResolverConverter<ProviderType>(),
            new TypeResolverConverter<ConnectionType>()
        };
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param> 
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/></param>
        public ConnectionRepository(string filePath, IEqualityComparer<string> comparer = null)
            : this(filePath, null, null, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public ConnectionRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                serializer.AddConverters(_requiredConverters);
                deserializer.AddConverters(_requiredConverters);
            });
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> for entity</param>
        public ConnectionRepository(string filePath,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
            ConfigureJsonSerializer((serializer, deserializer) =>
            {
                serializer.AddConverters(_requiredConverters);
                deserializer.AddConverters(_requiredConverters);
            });
        }

        /// <summary>
        ///     Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="user">The user.</param>
        public override void Add(IConnection entity, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Entities.Add(entity.Id, entity);
                Serialize();
            }
        }

        /// <summary>
        ///     Gets all entities.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public override IEnumerable<IConnection> GetAll(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                return InternalEntities.Values;
            }
        }

        /// <summary>
        ///     Gets the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public override IEnumerable<IConnection> Get(Expression<Func<IConnection, bool>> predicate, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return new List<IConnection>();
                }

                return InternalEntities.Values.AsQueryable().Where(predicate);
            }
        }

        /// <summary>
        ///     Removes the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        public override void Remove(Expression<Func<IConnection, bool>> predicate, ClaimsPrincipal user = null)
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

        public override Maybe<IConnection> Get(string id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return Maybe.Empty<IConnection>();
                }

                InternalEntities.TryGetValue(id, out var entity);
                return entity == null || entity.Equals(default(IConnection)) ? Maybe.Empty<IConnection>() : entity.ToMaybe();
            }
        }
    }
}