using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Threading;

namespace DHI.Services.Policies
{
    /// <summary>
    /// General retry policy.
    /// </summary>
    public class ExceptionRetryPolicy
    {
        private readonly RetryPolicy policy;

        /// <summary>
        /// Constructs an ExceptionRetryPolicy
        /// </summary>
        /// <param name="types">Array of Exception types to retry on.</param>
        /// <param name="logger">A logger.</param>
        public ExceptionRetryPolicy(Type[] types, ILogger logger)
            : this(types, new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger)
        {
        }

        /// <summary>
        /// Constructs an ExceptionRetryPolicy
        /// </summary>
        /// <param name="types">Array of Exception types to retry on.</param>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public ExceptionRetryPolicy(Type[] types, ILogger logger, string tag = "")
            : this(types, new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger)
        {
        }


        /// <summary>
        /// Constructs an ExceptionRetryPolicy
        /// </summary>
        /// <param name="types">Array of Exception types to retry on.</param>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public ExceptionRetryPolicy(Type[] types, TimeSpan[] waitTimes, ILogger logger, string tag = "")
        {
            IDisposable loggerState = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    loggerState = logger.BeginScope(tag);
                }

                policy = Policy
                    .Handle<Exception>(e => types.Contains(e.GetType()))
                    .WaitAndRetry(waitTimes,
                        (result, timeSpan, retryCount, context) => { logger?.LogInformation(result, "On retry {retryCount} after waiting {TotalSeconds}s after last attempt", retryCount, timeSpan.TotalSeconds); }
                    );
            }
            finally
            {
                loggerState?.Dispose();
            }
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        public TResult Execute<TResult>(Func<TResult> function)
        {
            return policy.Execute(function);
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the function.</returns>
        public TResult Execute<TResult>(Func<CancellationToken, TResult> function, CancellationToken cancellationToken)
        {
            return policy.Execute(function, cancellationToken);
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        public void Execute(Action action)
        {
            policy.Execute(action);
        }
    }
}