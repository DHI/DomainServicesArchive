namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public abstract class BaseJsonRepository<TEntity, TEntityId> : BaseDiscreteRepository<TEntity, TEntityId>
        where TEntity : IEntity<TEntityId>
    {
        private readonly string _filePath;
        private readonly FileInfo _fileInfo;
        private readonly IEqualityComparer<TEntityId> _comparer;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly JsonSerializerOptions _deserializerOptions;
        private readonly JsonConverter[] _requiredConverters = new JsonConverter[]
        {
            new JsonStringEnumConverter(),
            new Converters.EnumerationConverter(),
            new Converters.TypeStringConverter(),
            new Converters.ObjectToInferredTypeConverter(),
            new Converters.AutoNumberToStringConverter(),
            new Converters.PermissionConverter(),
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseJsonRepositoryDefault{TEntity, TEntityId}" /> class. 
        ///     Default <seealso cref="JsonSerializerOptions" /> will be used if not specify
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public BaseJsonRepository(string filePath)
            : this(filePath, null, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseJsonRepositoryDefault{TEntity, TEntityId}" /> class. 
        ///     Default <seealso cref="JsonSerializerOptions" /> will be used with <paramref name="converters"/> provided
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters">Create new instance <seealso cref="JsonSerializerOptions"/> with this <seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BaseJsonRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<TEntityId> comparer = null)
        {
            _comparer = comparer;
            _filePath = filePath?.TryResolveFullPath() ?? throw new ArgumentNullException(nameof(filePath));
            _fileInfo = new FileInfo(_filePath);

            _serializerOptions = _deserializerOptions = new JsonSerializerOptions();
            _serializerOptions
                .AddConverters(_requiredConverters)
                .AddConverters(converters);

            _deserializerOptions
                .AddConverters(_requiredConverters)
                .AddConverters(converters);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableJsonRepository{TEntity, TEntityId}" /> class.
        ///     Customized with user-specific <seealso cref="JsonSerializerOptions"/>
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer">Equality comparer</param>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public BaseJsonRepository(string filePath,
            JsonSerializerOptions serializerOptions = null,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<TEntityId> comparer = null)
        {
            _comparer = comparer;
            _filePath = filePath?.TryResolveFullPath() ?? throw new ArgumentNullException(nameof(filePath));
            _fileInfo = new FileInfo(_filePath);

            _serializerOptions = serializerOptions == null ? new JsonSerializerOptions() : new JsonSerializerOptions(serializerOptions);
            _deserializerOptions = deserializerOptions ?? new JsonSerializerOptions(_serializerOptions);

            _serializerOptions.Converters.Clear();
            _serializerOptions
                .AddConverters(_requiredConverters)
                .AddConverters(serializerOptions?.Converters);

            _deserializerOptions.Converters.Clear();
            _deserializerOptions
                .AddConverters(_requiredConverters)
                .AddConverters((deserializerOptions ?? _serializerOptions).Converters);
        }

        /// <summary>
        /// Configure exisiting <seealso cref="JsonSerializerOptions"/>
        /// </summary>
        /// <param name="configure">Action to <seealso cref="JsonSerializerOptions"/> to be configure</param>
        protected void ConfigureJsonSerializer(Action<JsonSerializerOptions> configure)
        {
            configure(_serializerOptions);
        }

        /// <summary>
        /// Configure exisiting <seealso cref="JsonSerializerOptions"/>
        /// </summary>
        /// <param name="configure">Action to configure <seealso cref="JsonSerializerOptions"/> for serializer and deserializer to be configures</param>
        protected void ConfigureJsonSerializer(Action<JsonSerializerOptions, JsonSerializerOptions> configure)
        {
            configure(_serializerOptions, _deserializerOptions);
        }

        /// <summary>
        ///     File path
        /// </summary>
        protected string FilePath => _filePath;

        /// <summary>
        ///     File info from <seealso cref="System.IO.FileInfo"/>
        /// </summary>
        protected FileInfo FileInfo => _fileInfo;

        /// <summary>
        ///     Entity ID comparer
        /// </summary>
        protected IEqualityComparer<TEntityId> Comparer => _comparer;

        ///// <summary>
        /////     Json Serializer options
        ///// </summary>
        //protected JsonSerializerOptions SerializerOptions => _serializerOptions;

        ///// <summary>
        /////     Json Deserializer options
        ///// </summary>
        //protected JsonSerializerOptions DeserializerOptions => _deserializerOptions;

        protected virtual void TrySerialize<T>(T entity)
        {
            using var streamWriter = new StreamWriter(_filePath);
            var json = JsonSerializer.Serialize(entity, _serializerOptions);
            streamWriter.Write(json);
        }

        protected virtual T TryDeserialize<T>()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    using var streamReader = new StreamReader(_filePath);
                    var json = streamReader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json) == false)
                    {
                        return JsonSerializer.Deserialize<T>(json, _deserializerOptions);
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"Cannot deserialize file {_filePath} with message {exception.Message}");
                }
            }

            return default;
        }
    }

    /// <summary>
    ///     Abstract class for repository based on json.
    ///     A json repository which accept a <see cref="JsonSerializerOptions"/> for serialization and deserializations of <typeparamref name="TEntity"/>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier.</typeparam>
    public abstract class BaseJsonRepositoryDefault<TEntity, TEntityId> : BaseJsonRepository<TEntity, TEntityId>
        where TEntity : IEntity<TEntityId>
    {
        private DateTime _lastModified = DateTime.MinValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseJsonRepositoryDefault{TEntity, TEntityId}" /> class. 
        ///     Default <seealso cref="JsonSerializerOptions" /> will be used if not specify
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public BaseJsonRepositoryDefault(string filePath)
            : this(filePath, null, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseJsonRepositoryDefault{TEntity, TEntityId}" /> class. 
        ///     Default <seealso cref="JsonSerializerOptions" /> will be used with <paramref name="converters"/> provided
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters">Create new instance <seealso cref="JsonSerializerOptions"/> with this <seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BaseJsonRepositoryDefault(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<TEntityId> comparer = null)
            : base(filePath, converters, comparer)
        {
            InternalEntities = new Dictionary<TEntityId, TEntity>(comparer);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableJsonRepository{TEntity, TEntityId}" /> class.
        ///     Customized with specific <seealso cref="JsonSerializerOptions"/>
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer">Equality comparer</param>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public BaseJsonRepositoryDefault(string filePath,
            JsonSerializerOptions serializerOptions = null,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<TEntityId> comparer = null) : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
            InternalEntities = new Dictionary<TEntityId, TEntity>(comparer);
        }

        protected Dictionary<TEntityId, TEntity> InternalEntities { get; set; }

        /// <summary>
        ///     Entities with timestamp modifier.
        ///     Accessing entities will auto update <see cref="FileInfo"/> last modified 
        /// </summary>
        protected virtual Dictionary<TEntityId, TEntity> Entities
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

        protected void Serialize() => TrySerialize<IDictionary<TEntityId, TEntity>>(InternalEntities);

        protected void Deserialize()
        {
            var entities = TryDeserialize<IDictionary<TEntityId, TEntity>>();
            if (entities != null)
            {
                InternalEntities = new Dictionary<TEntityId, TEntity>(entities, Comparer);
            }
        }
    }
}