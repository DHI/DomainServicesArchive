using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DHI.Services.Policies
{
    /// <summary>
    /// A retry policy intended for HttpClient operations.
    /// </summary>
    public class HttpRetryPolicy
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;

        /// <summary>
        /// Constructs an HttpRetryPolicy.
        /// </summary>
        /// <param name="logger">A Logger.</param>
        public HttpRetryPolicy(ILogger logger)
            : this(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger)
        {
        }

        /// <summary>
        /// Constructs an HttpRetryPolicy.
        /// </summary>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public HttpRetryPolicy(ILogger logger, string tag = "")
            : this(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger, tag)
        {
        }

        /// <summary>
        /// Constructs an HttpRetryPolicy.
        /// </summary>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A Logger.</param>
        /// <param name="tag">Add tag to apply to logs</param>
        public HttpRetryPolicy(TimeSpan[] waitTimes, ILogger logger, string tag = "")
        {
            IDisposable loggerState = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    loggerState = logger.BeginScope(tag);
                }

                _policy = Policy
                    .Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Unauthorized)
                    .WaitAndRetryAsync(waitTimes,
                        (result, timeSpan, retryCount, context) =>
                        {
                            if (result.Exception != null)
                            {
                                logger?.LogInformation(result.Exception, "On retry {retryCount} after waiting {timeSpan.TotalSeconds}s after last attempt: Failed HTTP request with code {result?.Result?.StatusCode}", retryCount, timeSpan.TotalSeconds, result?.Result?.StatusCode);
                            }
                            else
                            {
                                logger?.LogInformation("On retry {retryCount} after waiting {TotalSeconds}s after last attempt: Failed HTTP request with code {result?.Result?.StatusCode}", retryCount, timeSpan.TotalSeconds, result?.Result?.StatusCode);
                            }
                        });
            }
            finally
            {
                loggerState?.Dispose();
            }
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <param name="function">The function to execute.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Execute(Func<CancellationToken, Task<HttpResponseMessage>> function, CancellationToken cancellationToken)
        {
            return await _policy.ExecuteAsync(function, cancellationToken);
        }

        /// <summary>
        /// Execute function in retry policy.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>An HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> action)
        {
            return await _policy.ExecuteAsync(action);
        }
    }
}