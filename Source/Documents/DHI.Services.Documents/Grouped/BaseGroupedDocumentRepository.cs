namespace DHI.Services.Documents
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///   Abstract base class for a document repository
    /// </summary>
    public abstract class BaseGroupedDocumentRepository<TId> : BaseDiscreteRepository<Document<TId>, TId>, IGroupedDocumentRepository<TId>
    {
        /// <inheritdoc />
        public new abstract (Stream stream, string fileType, string fileName) Get(TId id, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract IDictionary<string, string> GetMetadata(TId id, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract IDictionary<TId, IDictionary<string, string>> GetMetadataByFilter(string filter, Parameters parameters = null, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract void Add(Stream stream, TId id, Parameters metadata, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract void Remove(TId id, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract bool ContainsGroup(string group, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public abstract IEnumerable<Document<TId>> GetByGroup(string group, ClaimsPrincipal user = null);

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(document => document.FullName).ToArray();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(document => document.FullName).ToArray();
        }

        /// <inheritdoc />
        public virtual IDictionary<TId, IDictionary<string, string>> GetAllMetadata(ClaimsPrincipal user = null)
        {
            return GetIds(user).ToDictionary(id => id, id => GetMetadata(id, user));
        }
    }
}