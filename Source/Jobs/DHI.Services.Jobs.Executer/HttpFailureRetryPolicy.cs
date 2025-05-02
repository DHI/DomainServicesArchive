namespace DHI.Services.Jobs.Executer;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Logging;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

public class HttpFailureRetryPolicy
{
    private const int _maxRetryCount = 5;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;

    public HttpFailureRetryPolicy(ILogger? logger)
    {
        var rng = new Random();
        var httpPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                _maxRetryCount,
                retryCount => TimeSpan.FromMilliseconds(rng.Next(50, 500) * Math.Pow(2, retryCount)),
                (result, timeSpan, retryCount, _) => logger?.LogInformation("On retry {Iteration} after waiting {WaitTime}s after last attempt: Failed to get authenticated with code {StatusCode} and message {Message}", retryCount, timeSpan.TotalSeconds, result?.Result?.StatusCode, result?.Exception?.Message)
            );

        IAsyncPolicy<HttpResponseMessage> fallbackPolicy =
            Policy<HttpResponseMessage>.Handle<Exception>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("Failed retry policy and catching all exceptions. Returning a 403 to handle outside this scope", Encoding.Default, "text/plain") // This is a hack to catch all exceptions and return a code to deal with outside of this scope
                });

        _policy = Policy.WrapAsync(fallbackPolicy, httpPolicy);
    }

    public async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> action)
    {
        return await _policy.ExecuteAsync(action);
    }
}