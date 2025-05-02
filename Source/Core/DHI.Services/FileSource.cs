namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Default File Source.
    /// </summary>
    /// <seealso cref="IFileSource" />
    [Serializable]
    public class FileSource : IFileSource
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSource" /> class.
        /// </summary>
        /// <param name="rootDirectoryPath">The root directory path.</param>
        public FileSource(string rootDirectoryPath = null)
        {
            RootDirectoryPath = rootDirectoryPath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(RootDirectoryPath) && !Directory.Exists(RootDirectoryPath))
            {
                throw new ArgumentException($"Specified root directory '{RootDirectoryPath}' does not exist");
            }
        }

        /// <summary>
        ///     Gets the root directory path.
        /// </summary>
        public string RootDirectoryPath { get; }

        /// <summary>
        ///     Deletes the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Delete(string filePath)
        {
            var concurrentFile = ConcurrentFile.GetFilePath(Path.Combine(RootDirectoryPath, filePath));
            File.Delete(concurrentFile);
        }

        /// <summary>
        ///     Checks if the specified file exists.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><c>true</c> if the file exists, <c>false</c> otherwise.</returns>
        public bool Exists(string filePath)
        {
            return File.Exists(Path.Combine(RootDirectoryPath, filePath));
        }

        /// <summary>
        ///     Gets the file paths within the specified path prefix.
        /// </summary>
        /// <param name="pathPrefix">The path prefix.</param>
        /// <param name="fileExtension">The file extension.</param>
        public IEnumerable<string> GetFilePaths(string pathPrefix, string fileExtension)
        {
            var filePathPrefix = Path.GetFullPath(Path.Combine(RootDirectoryPath, pathPrefix ?? string.Empty));

            if (!Directory.Exists(filePathPrefix))
            {
                return Array.Empty<string>();
            }

            var rootDirectoryPathLength = Path.GetFullPath(RootDirectoryPath).Length;
            return Directory.GetFiles(filePathPrefix, $"*{fileExtension}", SearchOption.AllDirectories)
                .Select(filePath => filePath.Substring(rootDirectoryPathLength, filePath.Length - rootDirectoryPathLength).TrimStart(Path.DirectorySeparatorChar));
        }

        /// <summary>
        ///     Gets the last write time for the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>DateTime.</returns>
        public DateTime GetLastWriteTime(string filePath)
        {
            var concurrentFile = ConcurrentFile.GetFilePath(Path.Combine(RootDirectoryPath, filePath));
            return File.GetLastWriteTime(concurrentFile);
        }

        /// <summary>
        ///     Saves the specified stream to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="stream">The file stream.</param>
        /// <param name="hidden">if set to <c>true</c> the file is marked as hidden.</param>
        public void Save(string filePath, Stream stream, bool hidden = false)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var fileStream = File.Open(Path.Combine(RootDirectoryPath, filePath), FileMode.Create))
            {
                stream.CopyTo(fileStream);
            }

            if (hidden)
            {
                File.SetAttributes(Path.Combine(RootDirectoryPath, filePath), FileAttributes.Hidden);
            }
        }

        /// <summary>
        ///     Gets the file paths.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        public IEnumerable<string> GetFilePaths(string fileExtension)
        {
            return GetFilePaths(null, fileExtension);
        }
    }
}