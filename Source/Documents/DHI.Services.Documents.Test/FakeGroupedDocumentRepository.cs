namespace DHI.Services.Documents.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    internal class FakeGroupedDocumentRepository : FakeDocumentRepository<string>, IGroupedDocumentRepository<string>
    {

        public FakeGroupedDocumentRepository()
        {
        }

        public FakeGroupedDocumentRepository(Dictionary<string, (Stream, Parameters)> documents) : base(documents)
        {
        }

        private Dictionary<FullName, Document> GroupedDocuments
        {
            get
            {
                var groupedDocuments = new Dictionary<FullName, Document>();
                foreach (var fullName in _documents.Select(valueTuple => FullName.Parse(valueTuple.Key)))
                {
                    groupedDocuments.Add(fullName, new Document(fullName.ToString(), fullName.Name, fullName.Group));
                }

                return groupedDocuments;
            }
        }

        public bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            return GroupedDocuments.Keys.Select(fullName => fullName.Group).Contains(group);
        }

        public IEnumerable<Document<string>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            var documents = new List<Document<string>>();
            foreach (var (key, value) in GroupedDocuments)
            {
                if (key.Group == group)
                {
                    documents.Add(value);
                }

            }

            return documents;
        }

        public IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group).Select(document => document.FullName).ToArray();
        }

        public IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll().Select(document => document.FullName).ToArray();
        }
    }
}