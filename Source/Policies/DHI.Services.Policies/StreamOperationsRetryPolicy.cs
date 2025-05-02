using Microsoft.Extensions.Logging;
using System;

namespace DHI.Services.Policies
{
    /// <summary>
    /// Retry policy intended for Stream operations.
    /// </summary>
    public class StreamOperationsRetryPolicy : AsyncExceptionRetryPolicy
    {
        private static readonly Type[] _types = new Type[]
        {
            typeof(ObjectDisposedException),
            typeof(InvalidOperationException)
        };

        /// <summary>
        /// Constructs a StreamOperationsRetryPolicy
        /// </summary>
        /// <param name="logger">A Logger</param>
        public StreamOperationsRetryPolicy(ILogger logger) : base(_types, logger)
        {
        }

        /// <summary>
        /// Constructs a StreamOperationsRetryPolicy
        /// </summary>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A Logger.</param>
        public StreamOperationsRetryPolicy(TimeSpan[] waitTimes, ILogger logger) : base(_types, waitTimes, logger)
        {
        }

        /// <summary>
        /// Constructs a StreamOperationsRetryPolicy
        /// </summary>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public StreamOperationsRetryPolicy(TimeSpan[] waitTimes, ILogger logger, string tag = "") : base(_types, waitTimes, logger)
        {
        }
    }
}