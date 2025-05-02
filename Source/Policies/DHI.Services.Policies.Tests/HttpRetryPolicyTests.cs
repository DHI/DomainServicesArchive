namespace DHI.Services.Policies.Tests
{
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using System.Net;

    public class HttpRetryPolicyTests
    {
        [Fact]
        public async Task HttpRetryPolicyIsOk()
        {
            var client = CreateMockHttpClient(HttpStatusCode.InternalServerError);
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retryPolicy = new HttpRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            await retryPolicy.ExecuteAsync(async () =>
            {
                i++;
                return await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost"));
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public async Task HttpRetryPolicyDoesNotRetryUnauthorized()
        {
            var client = CreateMockHttpClient(HttpStatusCode.Unauthorized);
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retryPolicy = new HttpRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            await retryPolicy.ExecuteAsync(async () =>
            {
                i++;
                return await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost"));
            });

            Assert.Equal(1, i);
        }

        [Fact]
        public void HttpRetryPolicyRetrysHttpExceptions()
        {
            var client = CreateMockHttpClient(HttpStatusCode.InternalServerError);
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retryPolicy = new HttpRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<HttpRequestException>(() =>
            {
                retryPolicy.ExecuteAsync(() =>
                {
                    i++;
                    throw new HttpRequestException();
                }).GetAwaiter().GetResult();
            });
            Assert.Equal(2, i);
        }

        private HttpClient CreateMockHttpClient(HttpStatusCode httpStatusCode)
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            mockMessageHandler.Protected()
                              .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                              .ReturnsAsync(new HttpResponseMessage
                              {
                                  StatusCode = httpStatusCode
                              }).Verifiable();

            return new HttpClient(mockMessageHandler.Object);
        }
    }
}
