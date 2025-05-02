namespace DHI.Services.Documents
{
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;

    /// <summary>
    ///     Interface IDocumentRepository
    /// </summary>
    /// <typeparam name="TId">The type of the document identifier.</typeparam>
    public interface IDocumentRepository<TId> : IDiscreteRepository<Document<TId>, TId>
    {

        /// <summary>
        ///     Gets a value tuple containing the stream of the document with the specified identifier, the file type and the name of the
        ///     attached file.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     A value tuple with the document stream, the file type (zip, png, tiff, gif, jpeg, bmp) and the name of the attached file.
        /// </returns>
        (Stream stream, string fileType, string fileName) Get(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///     Gets the metadata from the document with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        IDictionary<string, string> GetMetadata(TId id, ClaimsPrincipal user = null);

        /// <summary>
        ///    Get metadata of all documents          
        /// </summary>
        /// <param name="user">The user.</param>
        IDictionary<TId, IDictionary<string, string>> GetAllMetadata(ClaimsPrincipal user = null);

        /// <summary>
        ///    Searching the document's metadata based on with specific keyword/filter
        /// </summary>
        /// <param name="filter">The searching keyword.</param>
        /// <param name="user">The user.</param>
        /// <param name="parameters">Additional parameters.</param>
        IDictionary<TId, IDictionary<string, string>> GetMetadataByFilter(string filter, Parameters parameters = null, ClaimsPrincipal user = null);

        /// <summary>
        ///     Adds a document with the specified ID, containing the specified stream.
        /// </summary>
        /// <param name="stream">The document stream.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="metadata">The document metadata.</param>
        /// <param name="user">The user.</param>
        void Add(Stream stream, TId id, Parameters metadata, ClaimsPrincipal user = null);

        /// <summary>
        ///     Removes the document with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="user">The user.</param>
        void Remove(TId id, ClaimsPrincipal user = null);
    }
}