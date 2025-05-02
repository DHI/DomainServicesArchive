using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DHI.Services.Policies
{
    /// <summary>
    /// General retry policy for async processes
    /// </summary>
    public class AsyncExceptionRetryPolicy
    {
        private readonly AsyncRetryPolicy policy;

        /// <summary>
        /// Constructs an AsyncExceptionRetryPolicy
        /// </summary>
        /// <param name="types">Array of Exception types to retry on.</param>
        /// <param name="logger">A logger.</param>
        public AsyncExceptionRetryPolicy(Type[] types, ILogger logger)
            : this(types, new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger)
        {
        }

        /// <summary>
        /// Constructs an AsyncExceptionRetryPolicy
        /// </summary>
        /// <param name="types">Array of Exception types to retry on.</param>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public AsyncExceptionRetryPolicy(Type[] types, ILogger logger, string tag = "")
            : this(types, new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger)
        {
        }

        /// <summary>
        /// Constructs an AsyncExceptionRetryPolicy
        /// </summary>
        /// <param name="types">Array of Exception types to retry on.</param>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A Logger.</param>
        public AsyncExceptionRetryPolicy(Type[] types, TimeSpan[] waitTimes, ILogger logger)
        {
            policy = Policy
                .Handle<Exception>(e => types.Contains(e.GetType()))
                .WaitAndRetryAsync(waitTimes,
                    (result, timeSpan, retryCount, context) => { logger?.LogInformation(result, "On retry {retryCount} after waiting {TotalSeconds}s after last attempt", retryCount, timeSpan.TotalSeconds); }
                );
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> function)
        {
            return await policy.ExecuteAsync(function);
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the function.</returns>
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> function, CancellationToken cancellationToken)
        {
            return await policy.ExecuteAsync(function, cancellationToken);
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        public async Task ExecuteAsync<TResult>(Func<Task> function)
        {
            await policy.ExecuteAsync(function);
        }
    }
}