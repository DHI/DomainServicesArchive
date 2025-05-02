namespace DHI.Services.Jobs.Executer.Test;

using System.Net;
using Xunit;

public class RetryPolicyTest
{
    [Fact]
    public void FallbackPolicyIsOk()
    {
        var policy = new HttpFailureRetryPolicy(null);
        Assert.Equal(HttpStatusCode.Forbidden, policy.ExecuteAsync(() => throw new Exception("This should hit the fallback policy")).Result.StatusCode);
        Assert.Equal("Failed retry policy and catching all exceptions. Returning a 403 to handle outside this scope", policy.ExecuteAsync(() => throw new Exception("This should hit the fallback policy")).Result.Content.ReadAsStringAsync().Result);
    }
}