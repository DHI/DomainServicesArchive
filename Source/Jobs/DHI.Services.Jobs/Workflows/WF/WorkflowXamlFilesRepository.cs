namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.Unicode;

    [Obsolete("Use CodeWorkflowRepository instead. This type will eventually be removed.")]
    public class WorkflowXamlFilesRepository : IWorkflowRepository, ITaskRepository<Workflow, string>
    {
        private static readonly object SyncObject = new object();
        private readonly FileInfo _fileInfo;
        private readonly string _filePath;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly JsonSerializerOptions _deserializerOptions;
        private Dictionary<string, Workflow> _entities;
        private DateTime _lastModified = DateTime.MinValue;

        public WorkflowXamlFilesRepository(string filePath, IEnumerable<JsonConverter> converters = null)
        {
            Guard.Against.NullOrEmpty(filePath, nameof(filePath));
            _filePath = filePath;
            _fileInfo = new FileInfo(_filePath);

            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters =
                {
                    new JsonStringEnumConverter()
                },
            };
            _deserializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

            var defaultConverters = new List<JsonConverter>
            {
                new JsonStringEnumConverter(),
                new WorkflowXamlConverter()
            };
            if (converters != null)
            {
                foreach (JsonConverter converter in converters)
                {
                    _serializerOptions.Converters.Add(converter);
                    _deserializerOptions.Converters.Add(converter);
                }
            }
            else
            {
                foreach (var converter in defaultConverters)
                {
                    _serializerOptions.Converters.Add(converter);
                    _deserializerOptions.Converters.Add(converter);
                }
            }
        }

        private Dictionary<string, Workflow> Entities
        {
            get
            {
                _fileInfo.Refresh();
                if (_fileInfo.LastWriteTime == _lastModified)
                {
                    return _entities;
                }

                Deserialize();
                _lastModified = _fileInfo.LastWriteTime;
                return _entities;
            }
        }

        public int Count(ClaimsPrincipal user = null)
        {
            lock (SyncObject)
            {
                return Entities.Count;
            }
        }

        public bool Contains(string id, ClaimsPrincipal user = null)
        {
            lock (SyncObject)
            {
                return Entities.ContainsKey(id);
            }
        }

        public IEnumerable<string> GetIds(ClaimsPrincipal user = null)
        {
            lock (SyncObject)
            {
                return Entities.Values.Select(e => e.Id);
            }
        }

        public void Add(Workflow entity, ClaimsPrincipal user = null)
        {
            lock (SyncObject)
            {
                Entities.Add(entity.Id, entity);
                Serialize();
            }
        }

        public void Remove(string id, ClaimsPrincipal user = null)
        {
            lock (SyncObject)
            {
                if (Entities.Remove(id))
                {
                    var xamlFile = Path.Combine(Path.GetDirectoryName(_filePath), Path.GetFileNameWithoutExtension(_filePath), id + ".xaml");
                    if (File.Exists(xamlFile))
                    {
                        File.Delete(xamlFile);
                    }

                    Serialize();
                }
            }
        }

        public void Update(Workflow updatedEntity, ClaimsPrincipal user = null)
        {
            lock (SyncObject)
            {
                if (!_entities.ContainsKey(updatedEntity.Id))
                {
                    throw new KeyNotFoundException($"'{typeof(Workflow)}' with id '{updatedEntity.Id}' was not found.");
                }

                Entities[updatedEntity.Id] = updatedEntity;
                Serialize();
            }
        }

        Maybe<Workflow> IRepository<Workflow, string>.Get(string id, ClaimsPrincipal user)
        {
            lock (SyncObject)
            {
                Deserialize();
                if (!_entities.Any())
                {
                    return Maybe.Empty<Workflow>();
                }

                _entities.TryGetValue(id, out var entity);
                return entity == null || entity.Equals(default(Workflow)) ? Maybe.Empty<Workflow>() : entity.ToMaybe();
            }
        }

        public IEnumerable<Workflow> GetAll(ClaimsPrincipal user = null)
        {
            IEnumerable<Workflow> tasks;
            lock (SyncObject)
            {
                Deserialize();
                tasks = _entities.Values.ToArray();
            }

            return tasks;
        }

        public Maybe<ITask<string>> Get(string id)
        {
            ITask<string> task;
            lock (SyncObject)
            {
                Deserialize();
                if (!_entities.Any())
                {
                    return ((ITask<string>)null).ToMaybe();
                }

                _entities.TryGetValue(id, out var entity);
                task = entity == null || entity.Equals(default(Workflow)) ? null : entity;
            }

            return task?.ToMaybe() ?? Maybe.Empty<ITask<string>>();
        }

        public IEnumerable<ITask<string>> Get(Expression<Func<Workflow, bool>> predicate)
        {
            IEnumerable<ITask<string>> task;
            lock (SyncObject)
            {
                Deserialize();
                if (!_entities.Any())
                {
                    return new List<ITask<string>>();
                }

                task = _entities.Values.AsQueryable().Where(predicate);
            }

            return task;
        }

        public void Remove(Expression<Func<Workflow, bool>> predicate)
        {
            lock (SyncObject)
            {
                var toRemove = Get(predicate).ToList();
                foreach (var entity in toRemove)
                {
                    if (Entities.Remove(entity.Id))
                    {
                        var xamlFile = Path.Combine(Path.GetDirectoryName(_filePath), Path.GetFileNameWithoutExtension(_filePath), entity.Id + ".xaml");
                        if (File.Exists(xamlFile))
                        {
                            File.Delete(xamlFile);
                        }
                    }
                }

                Serialize();
            }
        }

        private void Serialize()
        {
            var folderPath = Path.Combine(Path.GetDirectoryName(_filePath), Path.GetFileNameWithoutExtension(_filePath));

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var entity in _entities)
            {
                using (var xamlStream = new FileStream(Path.Combine(folderPath, entity.Key + ".xaml"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (var xamlWriter = new StreamWriter(xamlStream))
                {
                    xamlStream.SetLength(0);
                    xamlStream.Flush();
                    xamlWriter.Write(entity.Value.Definition);
                }

                entity.Value.SetSerializeDefinition(false);
            }

            using (var jsonStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            using (var jsonWriter = new StreamWriter(jsonStream))
            {
                jsonStream.SetLength(0);
                jsonStream.Flush();
                var json = JsonSerializer.Serialize(_entities, _serializerOptions);
                jsonWriter.WriteLine(json);
            }
        }

        private void Deserialize()
        {
            if (!File.Exists(_filePath))
            {
                _entities = new Dictionary<string, Workflow>();
                return;
            }

            try
            {
                using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var json = streamReader.ReadToEnd();
                    _entities = new Dictionary<string, Workflow>(JsonSerializer.Deserialize<Dictionary<string, Workflow>>(json, _deserializerOptions));
                }

                var folderPath = Path.Combine(Path.GetDirectoryName(_filePath), Path.GetFileNameWithoutExtension(_filePath));

                if (!Directory.Exists(folderPath))
                {
                    throw new Exception($"Cannot deserialize xaml {folderPath}: folder does not exist");
                }

                foreach (var entity in _entities)
                {
                    using (var fileStream = new FileStream(Path.Combine(folderPath, entity.Key + ".xaml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        entity.Value.Definition = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Cannot deserialize file {_filePath} with message {exception.Message}");
            }
        }
    }
}