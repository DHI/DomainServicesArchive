#nullable enable
namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    ///     Abstract base class for a service.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    public abstract class BaseService<TEntity, TEntityId> : IService<TEntity, TEntityId> where TEntity : IEntity<TEntityId>
    {
        private readonly IRepository<TEntity, TEntityId> _repository;
        protected readonly ILogger? _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseService{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        protected BaseService(IRepository<TEntity, TEntityId> repository)
        {
            _logger = NullLogger.Instance;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseService{TEntity, TEntityId}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="logger">A logger</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        protected BaseService(IRepository<TEntity, TEntityId> repository, ILogger logger)
        {
            _logger = logger;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        ///     Gets the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>TEntity.</returns>
        [Obsolete("Use TryGet instead. This will be removed in a future version.")]
        public virtual TEntity Get(TEntityId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(id, user);
            if (!maybe.HasValue)
            {
                _logger?.LogError($"'{typeof(TEntity)}' with id '{id}' was not found.");
                throw new KeyNotFoundException($"'{typeof(TEntity)}' with id '{id}' was not found.");
            }

            return maybe.Value;
        }

        /// <summary>
        ///     Trys to get the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if entity was found, false otherwise</returns>
        public virtual bool TryGet(TEntityId id, out TEntity entity, ClaimsPrincipal user = null)
        {
            var maybe = _repository.Get(id, user);
            if (!maybe.HasValue)
            {
                _logger?.LogError($"'{typeof(TEntity)}' with id '{id}' was not found.");
                entity = default;
                return false;
            }

            entity = maybe.Value;
            return maybe.HasValue;
        }

        /// <summary>
        ///     Gets a list of entities with the specified identifiers.
        /// </summary>
        /// <remarks>
        ///     If an identifier is not found the method will log a warning
        /// </remarks>
        /// <param name="ids">The identifiers.</param>
        /// <param name="user">The user.</param>
        /// <returns>IEnumerable&lt;TEntity&gt;.</returns>
        [Obsolete("Use TryGet instead. This will be removed in a future version.")]
        public virtual IEnumerable<TEntity> Get(IEnumerable<TEntityId> ids, ClaimsPrincipal user = null)
        {
            foreach (var id in ids)
            {
                var maybe = _repository.Get(id, user);
                if (maybe.HasValue)
                {
                    yield return maybe.Value;
                }
                else
                {
                    _logger?.LogError($"'{typeof(TEntity)}' with id '{id}' was not found.");
                }
            }
        }

        /// <summary>
        ///     Trys to get a list of entities with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="user">The user.</param>
        /// <returns>True if all entities were found, false otherwise.</returns>
        public virtual bool TryGet(IEnumerable<TEntityId> ids, out IEnumerable<TEntity?> entities, ClaimsPrincipal user = null)
        {
            var results = new List<TEntity?>();
            var success = true;
            foreach (var id in ids)
            {
                var maybe = _repository.Get(id, user);
                if (maybe.HasValue)
                {
                    results.Add(maybe.Value);
                }
                else
                {
                    _logger?.LogError($"'{typeof(TEntity)}' with id '{id}' was not found.");
                    results.Add(default);
                    success = false;
                }
            }

            entities = results;
            return success;
        }
    }
}