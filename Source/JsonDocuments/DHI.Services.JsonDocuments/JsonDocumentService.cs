namespace DHI.Services.JsonDocuments
{
    using Authorization;
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Notifications;

    /// <summary>
    ///     Class JsonDocumentService.
    /// </summary>
    /// <typeparam name="TId">The type of the JsonDocument identifier.</typeparam>
    public class JsonDocumentService<TId> : BaseGroupedUpdatableDiscreteService<JsonDocument<TId>, TId>
    {
        private readonly INotificationRepository _logRepository;
        private readonly IJsonDocumentRepository<TId> _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocumentService{TId}" /> class.
        /// </summary>
        /// <param name="repository">The JsonDocument repository.</param>
        public JsonDocumentService(IJsonDocumentRepository<TId> repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocumentService{TId}" /> class.
        /// </summary>
        /// <param name="repository">The JsonDocument repository.</param>
        /// <param name="logRepository">A log repository.</param>
        public JsonDocumentService(IJsonDocumentRepository<TId> repository, INotificationRepository logRepository) : this(repository)
        {
            Guard.Against.Null(logRepository, nameof(logRepository));
            _logRepository = logRepository;
            Added += (_, args) =>
            {
                var (jsonDocument, userName) = args.Item;
                Log(jsonDocument, userName, "added");
            };

            Updated += (_, args) =>
            {
                var (jsonDocument, userName) = args.Item;
                Log(jsonDocument, userName, "updated");
            };
        }

        /// <summary>
        ///     Gets all notification entries associated with the given json document.
        /// </summary>
        /// <param name="jsonDocumentId">The json document identifier.</param>
        public IEnumerable<NotificationEntry> GetNotificationEntries(TId jsonDocumentId)
        {
            if (_logRepository is null)
            {
                throw new NotSupportedException(
                    "The JsonDocumentService is not configured for logging. This requires injecting an ILogRepository instance into the JsonDocumentService.");
            }

            return _logRepository.Get(new Query<NotificationEntry>(new QueryCondition("jsonDocumentId", jsonDocumentId)));
        }

        /// <summary>
        ///     Adds the given notification entry.
        ///     The log entry is associated to the given json document.
        /// </summary>
        /// <param name="jsonDocumentId">The json document identifier.</param>
        /// <param name="notificationEntry">The notification entry.</param>
        public void AddNotificationEntry(TId jsonDocumentId, NotificationEntry notificationEntry)
        {
            if (_logRepository is null)
            {
                throw new NotSupportedException(
                    "The JsonDocumentService is not configured for logging. This requires injecting an ILogRepository instance into the JsonDocumentService.");
            }

            var metadata = notificationEntry.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            metadata.Add("jsonDocumentId", jsonDocumentId);
            _logRepository.Add(new NotificationEntry(notificationEntry.Id, notificationEntry.NotificationLevel, notificationEntry.Text, notificationEntry.Source,
                notificationEntry.Tag, notificationEntry.MachineName, notificationEntry.DateTime, metadata));
        }

        /// <summary>
        ///     Gets json documents within the specified group.
        ///     The data is filtered by the given data selectors.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="dataSelectors">The data selectors.</param>
        /// <param name="user">The user.</param>
        public IEnumerable<JsonDocument<TId>> GetByGroup(string group, string[] dataSelectors,
            ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrEmpty(group, nameof(group));
            Guard.Against.Null(dataSelectors, nameof(dataSelectors));
            if (!_repository.ContainsGroup(group, user))
            {
                throw new KeyNotFoundException($"JSON Document group '{group}' does not exist.");
            }

            return _repository.GetByGroup(group, dataSelectors, user);
        }

        /// <summary>
        ///     Gets the json document with the specified identifier.
        ///     The data is filtered by the given data selectors.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dataSelectors">The data selectors.</param>
        /// <param name="user">The user.</param>
        public JsonDocument<TId> Get(TId id, string[] dataSelectors, ClaimsPrincipal user = null)
        {
            return _repository.Get(id, dataSelectors ?? Array.Empty<string>(), user).Value;
        }

        /// <summary>
        ///     Gets all json documents.
        /// </summary>
        /// <param name="dataSelectors">The data selectors for filtering.</param>
        /// <param name="user">The user.</param>
        public IEnumerable<JsonDocument<TId>> GetAll(string[] dataSelectors = null, ClaimsPrincipal user = null)
        {
            return _repository.GetAll(dataSelectors ?? Array.Empty<string>(), user);
        }

        /// <summary>
        ///     Gets all json documents meeting the criteria specified by the given query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="dataSelectors">The data selectors for filtering.</param>
        /// <param name="user">The user.</param>
        public IEnumerable<JsonDocument<TId>> Get(Query<JsonDocument<TId>> query, string[] dataSelectors = null,
            ClaimsPrincipal user = null)
        {
            return _repository.Get(query, dataSelectors ?? Array.Empty<string>(), user);
        }

        /// <summary>
        ///     Gets all json documents within the given time interval.
        /// </summary>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="dataSelectors">The data selectors.</param>
        /// <param name="user">The user.</param>
        public IEnumerable<JsonDocument<TId>> Get(DateTime from, DateTime to, string[] dataSelectors = null,
            ClaimsPrincipal user = null)
        {
            return _repository.Get(from, to, dataSelectors ?? Array.Empty<string>(), user);
        }

        /// <summary>
        ///     Adds the specified json document.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <param name="user">The user.</param>
        public override void Add(JsonDocument<TId> jsonDocument, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(jsonDocument.Id, user))
            {
                var cancelEventArgs = new CancelEventArgs<JsonDocument<TId>>(jsonDocument);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    if (jsonDocument is ITraceableEntity<TId> document)
                    {
                        document.Added = DateTime.UtcNow;
                        document.Updated = null;
                    }

                    _repository.Add(jsonDocument, user);
                    OnAdded(jsonDocument, user);
                }
            }
            else
            {
                throw new ArgumentException($"JsonDocument with id '{jsonDocument.Id}' already exists.");
            }
        }

        /// <summary>
        ///     Try adding the specified json document without existence check.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if json document was successfully added, <c>false</c> otherwise.</returns>
        public override bool TryAdd(JsonDocument<TId> jsonDocument, ClaimsPrincipal user = null)
        {
            try
            {
                var cancelEventArgs = new CancelEventArgs<JsonDocument<TId>>(jsonDocument);
                OnAdding(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                if (jsonDocument is ITraceableEntity<TId> document)
                {
                    document.Added = DateTime.UtcNow;
                    document.Updated = null;
                }

                _repository.Add(jsonDocument, user);
                OnAdded(jsonDocument, user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Updates the specified json document.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <param name="user">The user.</param>
        public override void Update(JsonDocument<TId> jsonDocument, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(jsonDocument.Id, user))
            {
                var cancelEventArgs = new CancelEventArgs<JsonDocument<TId>>(jsonDocument);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Update(jsonDocument, user);
                    OnUpdated(jsonDocument, user);
                }
            }
            else
            {
                throw new KeyNotFoundException($"JsonDocument with id '{jsonDocument.Id}' was not found.");
            }
        }

        /// <summary>
        ///     Try updating the specified json document without existence check.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if json document was successfully updated, <c>false</c> otherwise.</returns>
        public override bool TryUpdate(JsonDocument<TId> jsonDocument, ClaimsPrincipal user = null)
        {
            try
            {
                var cancelEventArgs = new CancelEventArgs<JsonDocument<TId>>(jsonDocument);
                OnUpdating(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                _repository.Update(jsonDocument, user);
                OnUpdated(jsonDocument, user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Adds or updates the specified json document.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <param name="user">The user.</param>
        public override void AddOrUpdate(JsonDocument<TId> jsonDocument, ClaimsPrincipal user = null)
        {
            if (!_repository.Contains(jsonDocument.Id, user))
            {
                var cancelEventArgs = new CancelEventArgs<JsonDocument<TId>>(jsonDocument);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    if (jsonDocument is ITraceableEntity<TId> document)
                    {
                        document.Added = DateTime.UtcNow;
                        document.Updated = null;
                    }

                    _repository.Add(jsonDocument, user);
                    OnAdded(jsonDocument, user);
                }
            }
            else
            {
                var cancelEventArgs = new CancelEventArgs<JsonDocument<TId>>(jsonDocument);
                OnUpdating(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Update(jsonDocument, user);
                    OnUpdated(jsonDocument, user);
                }
            }
        }

        /// <summary>
        ///     Removes the json document with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        public override void Remove(TId id, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(id, user))
            {
                var jsonDocument = _repository.Get(id, user) | default(JsonDocument<TId>);
                var cancelEventArgs = new CancelEventArgs<TId>(id);
                OnDeleting(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Remove(id, user);
                    OnDeleted(jsonDocument, user);
                }
            }
            else
            {
                throw new KeyNotFoundException($"JsonDocument with id '{id}' was not found.");
            }
        }

        /// <summary>
        ///     Try soft removing (mark as deleted) the json document with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if json document was successfully removed, <c>false</c> otherwise.</returns>
        public bool TrySoftRemove(TId id, ClaimsPrincipal user = null)
        {
            try
            {
                var cancelEventArgs = new CancelEventArgs<TId>(id);
                OnDeleting(cancelEventArgs);
                if (cancelEventArgs.Cancel)
                {
                    return false;
                }

                var jsonDocument = _repository.Get(id, user).Value;
                if (jsonDocument.Deleted is null)
                {
                    jsonDocument.Deleted = DateTime.UtcNow;
                    _repository.Update(jsonDocument, user);
                }

                OnDeleted(jsonDocument, user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Occurs when json document is added.
        /// </summary>
        public new event EventHandler<EventArgs<(JsonDocument<TId>, string)>> Added;

        /// <summary>
        ///     Occurs when json document is updated.
        /// </summary>
        public new event EventHandler<EventArgs<(JsonDocument<TId>, string)>> Updated;

        /// <summary>
        ///     Occurs when json document is deleted.
        /// </summary>
        public new event EventHandler<EventArgs<(JsonDocument<TId>, string)>> Deleted;

        private void Log(JsonDocument<TId> jsonDocument, string userName, string action)
        {
            var text = userName == null
                ? $"JSON document '{jsonDocument.Id}' was {action}."
                : $"JSON document '{jsonDocument.Id}' was {action} by '{userName}'.";
            var metadata = new Dictionary<string, object> { { "jsonDocumentId", jsonDocument.Id } };
            if (userName != null)
            {
                metadata.Add("userName", userName);
            }

            _logRepository.Add(new NotificationEntry(Guid.NewGuid(), NotificationLevel.Information, text, "JsonDocument Service",
                metadata: metadata));
        }

        private void OnAdded(JsonDocument<TId> jsonDocument, ClaimsPrincipal user)
        {
            var userName = user?.GetUserId();
            Added?.Invoke(this, new EventArgs<(JsonDocument<TId>, string)>((jsonDocument, userName)));
        }

        private void OnUpdated(JsonDocument<TId> jsonDocument, ClaimsPrincipal user)
        {
            var userName = user?.GetUserId();
            Updated?.Invoke(this, new EventArgs<(JsonDocument<TId>, string)>((jsonDocument, userName)));
        }

        private void OnDeleted(JsonDocument<TId> jsonDocument, ClaimsPrincipal user)
        {
            var userName = user?.GetUserId();
            Deleted?.Invoke(this, new EventArgs<(JsonDocument<TId>, string)>((jsonDocument, userName)));
        }
    }

    /// <inheritdoc />
    public class JsonDocumentService : JsonDocumentService<string>
    {
        /// <inheritdoc />
        public JsonDocumentService(IJsonDocumentRepository<string> repository)
            : base(repository)
        {
        }

        /// <inheritdoc />
        public JsonDocumentService(IJsonDocumentRepository<string> repository, INotificationRepository logRepository)
            : base(repository, logRepository)
        {
        }
    }
}