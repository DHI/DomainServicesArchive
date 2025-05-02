namespace DHI.Services.Documents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     Class DocumentService.
    /// </summary>
    /// <typeparam name="TId">The type of the document identifier.</typeparam>
    public class DocumentService<TId>
    {
        private readonly IDocumentRepository<TId> _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DocumentService{TId}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <exception cref="ArgumentNullException">repository</exception>
        public DocumentService(IDocumentRepository<TId> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Validators = new List<IValidator>();
        }

        /// <summary>
        ///     Occurs when putting a new document.
        /// </summary>
        public event EventHandler<CancelEventArgs<TId>> Putting;

        /// <summary>
        ///     Occurs when a document was removed.
        /// </summary>
        public event EventHandler<EventArgs<TId>> Removed;

        /// <summary>
        ///     Occurs when removing a document.
        /// </summary>
        public event EventHandler<CancelEventArgs<TId>> Removing;

        /// <summary>
        ///     Occurs when a document was put.
        /// </summary>
        public event EventHandler<EventArgs<TId>> WasPut;

        /// <summary>
        ///     Occurs when validating a document.
        /// </summary>
        public event EventHandler<EventArgs<Type>> Validating;

        /// <summary>
        ///     Occurs when a document has been validated.
        /// </summary>
        public event EventHandler<EventArgs<Type>> Validated;

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IDocumentRepository<TId>>(path);
        }

        /// <summary>
        ///     Gets the document validators.
        /// </summary>
        /// <value>The validators.</value>
        public ICollection<IValidator> Validators { get; }

        /// <summary>
        ///     Counts the number of documents.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>System.Int32.</returns>
        public int Count(ClaimsPrincipal user = null)
        {
            return _repository.Count(user);
        }

        /// <summary>
        ///     Determines whether a document with the specified identifier exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if document with the specified identifier exists, <c>false</c> otherwise.</returns>
        public bool Exists(TId id, ClaimsPrincipal user = null)
        {
            return _repository.Contains(id, user);
        }

        /// <summary>
        ///     Gets a value tuple containing the stream of the document with the specified identifier
        ///     as well as the file type and the name of the attached file.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     A value tuple with the document stream, the file type (zip, png, tiff, gif, jpeg, bmp) and the name of the attached file.
        /// </returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public (Stream stream, string fileType, string fileName) Get(TId id, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(id, user))
            {
                return _repository.Get(id, user);
            }

            throw new KeyNotFoundException($"Document with id '{id}' was not found.");
        }

        /// <summary>
        ///     Gets all documents.
        /// </summary>
        /// <param name="user">The user.</param>
        public IEnumerable<Document<TId>> GetAll(ClaimsPrincipal user = null)
        {
            return _repository.GetAll(user);
        }

        /// <summary>
        ///     Gets the document ids.
        /// </summary>
        /// <param name="user">The user.</param>
        public IEnumerable<TId> GetIds(ClaimsPrincipal user = null)
        {
            return _repository.GetIds(user);
        }

        /// <summary>
        ///     Gets the metadata from the document with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <exception cref="KeyNotFoundException"></exception>
        public IDictionary<string, string> GetMetadata(TId id, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(id, user))
            {
                return _repository.GetMetadata(id, user);
            }

            throw new KeyNotFoundException($"Document with id '{id}' was not found.");
        }

        /// <summary>
        ///     Gets the metadata from all documents.
        /// </summary>
        /// <param name="user">The user.</param>
        public IDictionary<TId, IDictionary<string, string>> GetAllMetadata(ClaimsPrincipal user = null)
        {
            return _repository.GetAllMetadata(user);
        }

        /// <summary>
        ///     Searching the document's metadata based on with specific keyword/filter
        /// </summary>
        /// <param name="filter">The searching keyword.</param>
        /// <param name="parameters">Additional parameters for searching metadata</param>
        /// <param name="user">The user.</param>
        public IDictionary<TId, IDictionary<string, string>> GetMetadataByFilter(string filter, Parameters parameters = null, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrEmpty(filter, nameof(filter));
            return _repository.GetMetadataByFilter(filter, parameters, user);
        }

        /// <summary>
        ///     Adds a document with the the specified ID, containing the specified stream.
        /// </summary>
        /// <param name="stream">The document stream.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="user">The user.</param>
        public void Add(Stream stream, TId id, Parameters parameters = null, ClaimsPrincipal user = null)
        {
            var fileName = parameters?.GetParameter("fileName", null);
            if (!(fileName is null))
            {
                foreach (var validator in Validators)
                {
                    if (!validator.CanValidate(fileName))
                    {
                        continue;
                    }

                    // Be a polite citizen and seek to the beginning of the stream
                    // before calling the next validator
                    stream.Seek(0, SeekOrigin.Begin);

                    OnValidating(validator.GetType());
                    var (validated, message) = validator.Validate(stream);
                    if (!validated)
                    {
                        throw new ArgumentException(message, nameof(stream));
                    }

                    OnValidated(validator.GetType());
                }
            }

            var cancelEventArgs = new CancelEventArgs<TId>(id);
            OnPutting(cancelEventArgs);
            if (!cancelEventArgs.Cancel)
            {
                // Be a polite citizen and seek to the beginning of the stream
                // before adding to the document repository
                stream.Seek(0, SeekOrigin.Begin);

                _repository.Add(stream, id, parameters, user);
                OnPut(id);
            }
        }

        /// <summary>
        ///     Adds a document with the the specified ID, reading from the specified file.
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="id">The identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="user">The user.</param>
        public void Add(string filePath, TId id, Parameters parameters = null, ClaimsPrincipal user = null)
        {
            Guard.Against.Null(filePath, nameof(filePath));
            parameters ??= new Parameters();
            if (!parameters.ContainsKey("fileName"))
            {
                parameters.Add("fileName", Path.GetFileName(filePath));
            }

            Add(new MemoryStream(File.ReadAllBytes(filePath)), id, parameters, user);
        }

        /// <summary>
        ///     Removes the document with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <exception cref="KeyNotFoundException"></exception>
        public void Remove(TId id, ClaimsPrincipal user = null)
        {
            if (_repository.Contains(id, user))
            {
                var cancelEventArgs = new CancelEventArgs<TId>(id);
                OnRemoving(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Remove(id, user);
                    OnRemoved(id);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Document with id '{id}' was not found.");
            }
        }

        /// <summary>
        ///     Called when document was put.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected virtual void OnPut(TId id)
        {
            WasPut?.Invoke(this, new EventArgs<TId>(id));
        }

        /// <summary>
        ///     Called when putting document.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnPutting(CancelEventArgs<TId> e)
        {
            Putting?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a document was removed.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected virtual void OnRemoved(TId id)
        {
            Removed?.Invoke(this, new EventArgs<TId>(id));
        }

        /// <summary>
        ///     Called when removing a document.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnRemoving(CancelEventArgs<TId> e)
        {
            Removing?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when a document was validated.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        protected virtual void OnValidated(Type validatorType)
        {
            Validated?.Invoke(this, new EventArgs<Type>(validatorType));
        }

        /// <summary>
        ///     Called when validating a document.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        protected virtual void OnValidating(Type validatorType)
        {
            Validating?.Invoke(this, new EventArgs<Type>(validatorType));
        }
    }

    /// <inheritdoc />
    public class DocumentService : DocumentService<string>
    {
        /// <inheritdoc />
        public DocumentService(IDocumentRepository<string> repository) : base(repository)
        {
        }
    }
}