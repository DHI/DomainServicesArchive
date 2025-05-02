namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///     Interface for a generic file source (Windows, Azure Blob Storage etc.)
    /// </summary>
    public interface IFileSource
    {
        /// <summary>
        ///     Gets the file paths within the specified path prefix.
        /// </summary>
        /// <param name="pathPrefix">The path prefix.</param>
        /// <param name="fileExtension">The file extension.</param>
        IEnumerable<string> GetFilePaths(string pathPrefix, string fileExtension);

        /// <summary>
        ///     Saves the specified stream to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileStream">The file stream.</param>
        /// <param name="hidden">if set to <c>true</c> the file is marked as hidden.</param>
        void Save(string filePath, Stream fileStream, bool hidden = false);

        /// <summary>
        ///     Deletes the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void Delete(string filePath);

        /// <summary>
        ///     Checks if the specified file exists.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><c>true</c> if the file exists, <c>false</c> otherwise.</returns>
        bool Exists(string filePath);

        /// <summary>
        ///     Gets the last write time for the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        DateTime GetLastWriteTime(string filePath);
    }
}