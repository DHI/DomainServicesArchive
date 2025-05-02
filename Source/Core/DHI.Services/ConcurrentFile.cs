namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     The purpose of the ConcurrentFile functionality is to avoid providers and consumers of files to clash in case a provider of a file is trying to update a file that's currently in use.
    ///     This issue occurs more frequently the larger the files are and the more frequently they are accessed.
    ///     The functionality is meant to be used in both the provider and the consumer end where the consumer will uses the method to indicate what file name
    ///     e.g. C:\xyz.txt should be made concurrent and the functionality responds back with a file name C:\xyz yyyy-MM-dd-HH-mm-ss.txt.$$$.
    ///     The $$$ is an indication that the file is going to be created, but not ready for use yet. The provider then uses this file name to copy or create the file.
    ///     At the same time consumers are asking for the C:\xyz.txt file. The functionality will first attempt to rename all files in the folder with $$$ removing the $$$.
    ///     Success indicates that the file has successfully been created and is ready for consumption, failure means its still in use by the provider.
    ///     After this the consumer lists all the files matching the C:\xyz yyyy-MM-dd-HH-mm-ss.txt pattern and returns the latest of these files.
    ///     It also tries to delete all files matching this pattern except for the latest file. If it succeeds it means no consumers are using older files.
    ///     If it fails, it means consumers are still using older files which will be freed up and deleted in a later attempt by another consumer.
    /// </summary>
    public static class ConcurrentFile
    {
        /// <summary>
        ///     Gets the file path of current concurrent file or creates a new concurrent file path.
        /// </summary>
        /// <param name="filePath">The full path to the file to ask for.</param>
        /// <param name="newFile">Indicates if the file should be new.</param>
        /// <returns>The real file path.</returns>
        public static string GetFilePath(string filePath, bool newFile = false)
        {
            Guard.Against.NullOrEmpty(filePath, nameof(filePath));
            var directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("The given file path is not valid. Either it contains no directory information or it is a root.", nameof(filePath));
            }

            var fileNoExtension = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempFiles = Directory.GetFileSystemEntries(directory).Where(entry => entry.StartsWith(Path.Combine(directory, fileNoExtension + " ")) && entry.EndsWith(".$$$"));
            foreach (var tempFile in tempFiles)
            {
                try
                {
                    File.Move(tempFile, tempFile.Replace(".$$$", string.Empty));
                }
                catch
                {
                    // ignored
                }
            }

            var actualFiles = Directory.GetFileSystemEntries(directory).Where(r => r.StartsWith(Path.Combine(directory, fileNoExtension + " ")) && r.EndsWith(extension));
            var items = new List<KeyValuePair<DateTime, string>>();
            foreach (var actualFile in actualFiles)
            {
                var dateTimeString = actualFile.Replace(Path.Combine(directory, fileNoExtension + " "), string.Empty).Replace(extension, string.Empty);
                if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    items.Add(new KeyValuePair<DateTime, string>(dateTime, actualFile));
                }
            }

            items = items.OrderBy(r => r.Key).ToList();

            if (items.Count >= 1)
            {
                foreach (var item in items.Take(items.Count - 1))
                {
                    try
                    {
                        File.Delete(item.Value);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            if (newFile)
            {
                return Path.Combine(directory, fileNoExtension + " " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + extension + ".$$$");
            }

            return items.Count > 0 ? items.Last().Value : filePath;
        }
    }
}