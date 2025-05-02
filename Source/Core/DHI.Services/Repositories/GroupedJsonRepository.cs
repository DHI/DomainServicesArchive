namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Generic grouped JSON repository.
    ///     All entities must belong to a group.
    ///     The grouping is one level only (not hierarchical).
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class GroupedJsonRepository<TEntity> :
        BaseJsonRepository<TEntity, string>,
        IGroupedRepository<TEntity>,
        IUpdatableRepository<TEntity, string> where TEntity : IGroupedEntity<string>
    {
        private static readonly object _syncObject = new();
        private DateTime _lastModified = DateTime.MinValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedJsonRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public GroupedJsonRepository(string filePath)
            : this(filePath, null, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedJsonRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer">Equality comparer for <typeparamref name="TEntity"/></param>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public GroupedJsonRepository(string filePath,
            JsonSerializerOptions serializerOptions = null,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
            InternalEntities = new Dictionary<string, IDictionary<string, TEntity>>(Comparer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedJsonRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer</param>
        public GroupedJsonRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
            InternalEntities = new Dictionary<string, IDictionary<string, TEntity>>(Comparer);
        }

        protected IDictionary<string, IDictionary<string, TEntity>> InternalEntities { get; set; }

        /// <summary>
        ///     Entities with timestamp modifier.
        ///     Accessing entities will auto update <see cref="FileInfo"/> last modified 
        /// </summary>
        protected IDictionary<string, IDictionary<string, TEntity>> Entities
        {
            get
            {
                FileInfo.Refresh();
                if (FileInfo.LastWriteTime == _lastModified)
                {
                    return InternalEntities;
                }

                Deserialize();
                _lastModified = FileInfo.LastWriteTime;
                return InternalEntities;
            }
        }

        /// <summary>
        ///     The total number of entities.
        /// </summary>
        /// <param name="user">The user.</param>
        public override int Count(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var count = 0;
                foreach (var group in Entities)
                {
                    count += group.Value.Count;
                }

                return count;
            }
        }

        /// <summary>
        ///     Determines whether an entity with the specified fullname identifier exists.
        /// </summary>
        /// <param name="id">The fullname identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if entity with the specified fullname identifier exists, <c>false</c> otherwise.</returns>
        public override bool Contains(string id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                try
                {
                    var fullName = TryGetFullName(id, user);
                    return Entities.ContainsKey(fullName.Group) && Entities[fullName.Group].ContainsKey(fullName.Name);
                }
                catch
                {
                    //error while trying get fullname, simply return false
                    return false;
                }
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
                var entities = new List<TEntity>();
                foreach (var group in InternalEntities)
                {
                    foreach (var entity in group.Value.Values)
                    {
                        entities.Add(entity);
                    }
                }

                return entities;
            }
        }

        /// <summary>
        ///     Gets all entity identifiers.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;string&gt;.</returns>
        public override IEnumerable<string> GetIds(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var ids = new List<string>();
                foreach (var group in Entities)
                {
                    foreach (var entity in group.Value.Values)
                    {
                        ids.Add(entity.Id);
                    }
                }

                return ids;
            }
        }

        /// <summary>
        ///     Determines whether the repository contains the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the repository contains the specified group; otherwise, <c>false</c>.</returns>
        public bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                return Entities.ContainsKey(group);
            }
        }

        /// <summary>
        ///     Gets entities by group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        public IEnumerable<TEntity> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                Deserialize();
                return InternalEntities[group].Values.ToArray();
            }
        }

        /// <summary>
        ///     Gets the full names by group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                return Entities[group].Values.Select(e => e.FullName).ToArray();
            }
        }

        /// <summary>
        ///     Gets the full names.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        public IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var fullNames = new List<string>();
                foreach (var group in Entities)
                {
                    foreach (var entity in group.Value.Values)
                    {
                        fullNames.Add(entity.FullName);
                    }
                }

                return fullNames;
            }
        }

        /// <summary>
        ///     Gets the entity with the specified fullname identifier.
        /// </summary>
        /// <param name="id">The fullname identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>Maybe&lt;TEntity&gt;.</returns>
        public override Maybe<TEntity> Get(string id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var fullName = TryGetFullName(id, user);
                Deserialize();
                if (!InternalEntities.Any())
                {
                    return Maybe.Empty<TEntity>();
                }

                InternalEntities.TryGetValue(fullName.Group, out var group);
                if (group is null)
                {
                    return Maybe.Empty<TEntity>();
                }

                group.TryGetValue(fullName.Name, out var entity);
                return entity == null || entity.Equals(default(TEntity)) ? Maybe.Empty<TEntity>() : entity.ToMaybe();
            }
        }

        /// <summary>
        ///     Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="user">The user.</param>
        public void Add(TEntity entity, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                if (entity.Group is null)
                {
                    throw new ArgumentException($"The entity '{entity}' does not belong to a group.", nameof(entity));
                }

                if (!Entities.ContainsKey(entity.Group))
                {
                    Entities.Add(entity.Group, new Dictionary<string, TEntity>(Comparer));
                }

                var group = Entities[entity.Group];
                //if (group.ContainsKey(entity.Name))
                //    group[entity.Name] = entity;
                //else
                group.Add(entity.Name, entity);

                Serialize();
            }
        }

        /// <summary>
        ///     Removes the entity with the specified fullname identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        public void Remove(string id, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var fullName = TryGetFullName(id, user);
                Entities.TryGetValue(fullName.Group, out var group);
                if (group is null)
                {
                    return;
                }

                if (!group.Remove(fullName.Name))
                {
                    return;
                }

                if (group.Count == 0)
                {
                    Entities.Remove(fullName.Group);
                }

                Serialize();
            }
        }

        /// <summary>
        ///     Updates the specified entity.
        /// </summary>
        /// <param name="updatedEntity">The updated entity.</param>
        /// <param name="user">The user.</param>
        public void Update(TEntity updatedEntity, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var group = Entities[updatedEntity.Group];
                var current = group.ContainsKey(updatedEntity.Name) ?
                    group[updatedEntity.Name] :
                    group.First(x => x.Value.Id == updatedEntity.Id).Value;

                if (updatedEntity is ITraceableEntity<string> entity)
                {
                    entity.Added = group[current.Name].Added;
                }

                group.Remove(current.Name); //removed existing key
                group[updatedEntity.Name] = updatedEntity;
                Serialize();
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
                var entities = new List<TEntity>();
                if (!InternalEntities.Any())
                {
                    return entities;
                }

                foreach (var group in InternalEntities)
                {
                    foreach (var entity in group.Value.Values.AsQueryable().Where(predicate))
                    {
                        entities.Add(entity);
                    }
                }

                return entities;
            }
        }

        /// <summary>
        ///     Removes the entities fulfilling the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="user">The user.</param>
        public void Remove(Expression<Func<TEntity, bool>> predicate, ClaimsPrincipal user = null)
        {
            lock (_syncObject)
            {
                var toRemove = Get(predicate);
                foreach (var entity in toRemove)
                {
                    Remove(entity.FullName);
                }
            }
        }

        private FullName TryGetFullName(string id, ClaimsPrincipal user = null)
        {
            var fullName = FullName.Parse(id);
            if (fullName.Group is null)
            {
                fullName = tryGetFullNameById(id, user);
                if (fullName.Group is null)
                    throw new ArgumentException($"Invalid ID '{id}'. The ID of a grouped entity must be a string with following format: {{group}}/{{name}}.", nameof(id));
            }
            return fullName;

            //Considered 'Id' is not fullName for a GroupedEntity, try get FullName based the given Id
            FullName tryGetFullNameById(string id, ClaimsPrincipal user = null)
            {
                var entities = Get(entity => entity.Id == id, user);
                if (entities == null || entities.Any() == false || entities.Count() > 1)
                    throw new ArgumentException($"GroupedEntity with ID: '{id}' either cannot be found, user not authorized or there is duplicate entity on different group. The ID of a grouped entity must be a string with following format: {{group}}/{{name}}. Note: For a grouped entity, please use 'FullName' instead of 'Id' ");

                return FullName.Parse(entities.First().FullName);
            }
        }
        protected void Serialize() => TrySerialize(InternalEntities);

        protected void Deserialize()
        {
            var entities = TryDeserialize<IDictionary<string, IDictionary<string, TEntity>>>();
            if (entities != null)
            {
                //insert comparer on child dictionary
                foreach (var dictionary in entities)
                {
                    entities[dictionary.Key] = new Dictionary<string, TEntity>(dictionary.Value, Comparer);
                }

                InternalEntities = new Dictionary<string, IDictionary<string, TEntity>>(entities, Comparer);
            }
        }
    }
}