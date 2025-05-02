namespace DHI.Services
{
    using System.IO;

    /// <summary>
    ///     Interface for a streamable file source (Azure Blob Storage etc.)
    /// </summary>
    public interface IStreamableFileSource : IFileSource
    {
        /// <summary>
        ///     Opens the file for reading.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>a stream.</returns>
        Stream OpenRead(string filePath);
    }
}
