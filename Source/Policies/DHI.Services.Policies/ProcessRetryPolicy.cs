using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DHI.Services.Policies
{
    /// <summary>
    /// Retry policy for Systems.Diagnostic.Process
    /// </summary>
    public class ProcessRetryPolicy
    {
        private readonly ILogger _logger;
        private readonly TimeSpan _overallTimeout;
        private readonly AsyncRetryPolicy<Process> policy;

        /// <summary>
        /// Constructs a ProcessRetryPolicy
        /// </summary>
        /// <param name="overallTimeout">Maximum allowable duration of Process runtime.</param>
        /// <param name="logger">A Logger.</param>
        public ProcessRetryPolicy(TimeSpan overallTimeout, ILogger logger)
            : this(overallTimeout, new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger)
        {
        }

        /// <summary>
        /// Constructs a ProcessRetryPolicy
        /// </summary>
        /// <param name="overallTimeout">Maximum allowable duration of Process runtime.</param>
        /// <param name="logger">A DHI.Services.ILogger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public ProcessRetryPolicy(TimeSpan overallTimeout, ILogger logger, string tag = "")
            : this(overallTimeout, new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(120) }, logger, tag)
        {
        }

        /// <summary>
        /// Constructs a ProcessRetryPolicy
        /// </summary>
        /// <param name="overallTimeout">Maximum allowable duration of Process runtime.</param>
        /// <param name="waitTimes">Array of timespan describing retry pattern.</param>
        /// <param name="logger">A Logger.</param>
        /// <param name="tag">Tag value to apply to logs.</param>
        public ProcessRetryPolicy(TimeSpan overallTimeout, TimeSpan[] waitTimes, ILogger logger, string tag = "")
        {
            IDisposable loggerState = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    loggerState = logger.BeginScope(tag);
                }

                _logger = logger;
                _overallTimeout = overallTimeout;
                policy = Policy
                    .Handle<Exception>()
                    .OrResult<Process>(response => response.ExitCode != 0)
                    .WaitAndRetryAsync(
                        waitTimes,
                        (result, timeSpan, retryCount, context) =>
                        {
                            if (result.Exception != null)
                            {
                                _logger?.LogInformation(result.Exception, "On retry {retryCount} after waiting {timeSpan.TotalSeconds}s after last attempt: Failed with code {result.Result.ExitCode}", retryCount, timeSpan.TotalSeconds, result.Result.ExitCode);
                            }
                            else
                            {
                                _logger?.LogInformation("On retry {retryCount} after waiting {timeSpan.TotalSeconds}s after last attempt: Failed with code {result.Result.ExitCode}", retryCount, timeSpan.TotalSeconds, result.Result.ExitCode);
                            }

                            var standardOutput = result.Result.StandardOutput.ReadToEnd();

                            if (!string.IsNullOrEmpty(standardOutput))
                            {
                                _logger?.LogInformation(standardOutput.Substring(0, Math.Min(standardOutput.Length, 1000)));
                            }
                        });
            }
            finally
            {
                loggerState?.Dispose();
            }
        }

        /// <summary>
        /// Executes process in retry policy.
        /// </summary>
        /// <remarks>
        /// The process should be constructed and disposed outside this function. The action should only contain those parts of the process launch that can be repeated.
        /// </remarks>
        /// <param name="process">The process to execute.</param>
        /// <param name="action">Function to execute in retry policy.</param>
        /// <returns>The process.</returns>
        public async Task<Process> ExecuteAsync(Process process, Func<Task<Process>> action)
        {
            return await policy.ExecuteAsync(async () =>
            {
                using (var timer2 = new Timer(TimeoutHandler, process, (int)_overallTimeout.TotalMilliseconds, Timeout.Infinite))
                {
                    var returnProcess = await action();
                    return returnProcess;
                }
            });
        }

        private void TimeoutHandler(object arg)
        {
            var process = arg as Process;
            if (process != null && !process.HasExited)
            {
                process?.Kill();
                _logger?.LogWarning("Process {name} {argument} killed due to timeout of {timeout} minutes", process.StartInfo?.FileName, process.StartInfo?.Arguments, _overallTimeout.TotalMinutes);
            }
            else
            {
                _logger?.LogWarning("Null process killed due to timeout of {timeout} minutes", _overallTimeout);
            }
        }
    }
}