namespace DHI.Services.Documents.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    internal class FakeDocumentRepository<TId> : IDocumentRepository<TId>
    {
        protected readonly Dictionary<TId, (Stream stream, Parameters metadata)> _documents;

        public FakeDocumentRepository()
        {
            _documents = new Dictionary<TId, (Stream, Parameters)>();
        }

        public FakeDocumentRepository(Dictionary<TId, (Stream, Parameters)> documents)
        {
            _documents = documents;
        }

        public int Count(ClaimsPrincipal user = null)
        {
            return _documents.Count;
        }

        public bool Contains(TId id, ClaimsPrincipal user = null)
        {
            return _documents.ContainsKey(id);
        }

        public IEnumerable<Document<TId>> GetAll(ClaimsPrincipal user = null)
        {
            return _documents.Select(valueTuple => new Document<TId>(valueTuple.Key, valueTuple.Key.ToString())).ToArray();
        }

        public (Stream stream, string fileType, string fileName) Get(TId id, ClaimsPrincipal user = null)
        {
            _documents.TryGetValue(id, out var document);
            return (document.stream, null, null);
        }

        public IEnumerable<TId> GetIds(ClaimsPrincipal user = null)
        {
            return _documents.Keys.ToArray();
        }

        public IDictionary<string, string> GetMetadata(TId id, ClaimsPrincipal user = null)
        {
            return _documents[id].metadata;
        }

        public IDictionary<TId, IDictionary<string, string>> GetMetadataByFilter(string filter, Parameters parameters = null, ClaimsPrincipal user = null)
        {
            var result = new Dictionary<TId, IDictionary<string, string>>();
            foreach (var (key, value) in _documents.Where(s => s.Value.metadata.Values.Any(x => x.Contains(filter))))
            {
                var metadata = new Dictionary<string, string>();
                foreach (var param in value.metadata)
                {
                    metadata.Add(param.Key, param.Value);
                }

                result.Add(key, metadata);
            }

            return result;
        }

        public void Add(Stream document, TId id, Parameters metadata, ClaimsPrincipal user = null)
        {
            _documents[id] = (document, metadata);
        }

        public void Remove(TId id, ClaimsPrincipal user = null)
        {
            _documents.Remove(id);
        }

        public IDictionary<TId, IDictionary<string, string>> GetAllMetadata(ClaimsPrincipal user = null)
        {
            var result = new Dictionary<TId, IDictionary<string, string>>();
            foreach (var (key, value) in _documents.Where(s => s.Value.metadata.Values.Any()))
            {
                var metadata = new Dictionary<string, string>();
                foreach (var param in value.metadata)
                {
                    metadata.Add(param.Key, param.Value);
                }

                result.Add(key, metadata);
            }

            return result;

        }
    }
}