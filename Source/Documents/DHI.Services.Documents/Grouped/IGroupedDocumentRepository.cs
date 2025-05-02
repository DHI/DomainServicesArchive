namespace DHI.Services.Documents
{
    /// <summary>
    ///     Interface IGroupedDocumentRepository
    /// </summary>
    /// <typeparam name="TId">The type of the document identifier.</typeparam>
    public interface IGroupedDocumentRepository<TId> : IDocumentRepository<TId>, IGroupedRepository<Document<TId>>
    {
    }
}