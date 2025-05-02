using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace DHI.Services.Policies
{
    /// <summary>
    /// Retry policy intended for file access operations.
    /// </summary>
    public class FileAccessRetryPolicy : ExceptionRetryPolicy
    {
        private static readonly Type[] _types = new Type[]
        {
            typeof(DirectoryNotFoundException),
            typeof(FileNotFoundException),
            typeof(IOException)
        };

        /// <summary>
        /// Constructs a FileAccessRetryPolicy.
        /// </summary>
        /// <param name="logger">A Logger.</param>
        public FileAccessRetryPolicy(ILogger logger) : base(_types, logger)
        {
        }

        /// <summary>
        /// Constructs a FileAccessRetryPolicy.
        /// </summary>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A ILogger.</param>
        public FileAccessRetryPolicy(TimeSpan[] waitTimes, ILogger logger) : base(_types, waitTimes, logger)
        {
        }

        /// <summary>
        /// Constructs a FileAccessRetryPolicy.
        /// </summary>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public FileAccessRetryPolicy(TimeSpan[] waitTimes, ILogger logger, string tag = "") : base(_types, waitTimes, logger)
        {
        }
    }
}
