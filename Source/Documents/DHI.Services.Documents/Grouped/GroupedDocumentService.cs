namespace DHI.Services.Documents
{
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    ///     Class GroupedDocumentService.
    /// </summary>
    /// <typeparam name="TId">The type of the document identifier.</typeparam>
    public class GroupedDocumentService<TId> : DocumentService<TId>
    {
        private readonly IGroupedDocumentRepository<TId> _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedDocumentService{TId}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public GroupedDocumentService(IGroupedDocumentRepository<TId> repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Gets the documents by group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="user">The user.</param>
        public virtual IEnumerable<Document<TId>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrEmpty(group, nameof(group));
            if (!_repository.ContainsGroup(group, user))
            {
                throw new KeyNotFoundException($"Group '{group}' does not exist.");
            }

            return _repository.GetByGroup(group, user);
        }

        /// <summary>
        ///     Gets the documents in each group.
        /// </summary>
        /// <param name="groups">The list of groups</param>
        /// <param name="user">The user.</param>
        public virtual IEnumerable<Document<TId>> GetByGroups(IEnumerable<string> groups, ClaimsPrincipal user = null)
        {
            var list = new List<Document<TId>>();
            foreach (var group in groups)
            {
                list.AddRange(GetByGroup(group, user));
            }

            return list;
        }

        /// <summary>
        ///     Gets the full names within the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="user">The user.</param>
        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrEmpty(group, nameof(group));
            if (!_repository.ContainsGroup(group, user))
            {
                throw new KeyNotFoundException($"Group '{group}' does not exist.");
            }

            return _repository.GetFullNames(group, user);
        }

        /// <summary>
        ///     Gets all the full names.
        /// </summary>
        /// <param name="user">The user.</param>
        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return _repository.GetFullNames(user);
        }
    }

    public class GroupedDocumentService : GroupedDocumentService<string>
    {
        public GroupedDocumentService(IGroupedDocumentRepository<string> repository) : base(repository)
        {
        }
    }
}