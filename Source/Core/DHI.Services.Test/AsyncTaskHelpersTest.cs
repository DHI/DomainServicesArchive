namespace DHI.Services.Test
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class AsyncTaskHelpersTest
    {
        [Fact]
        public void ThrowInRunBlockStillThrows()
        {
            var ex = Assert.Throws<Exception>(() => AsyncHelpers.RunSync(DoWorkException));
            Assert.Equal("This is an exception", ex.Message);
        }

        [Fact]
        public async Task DoWorkAsyncIsOk()
        {
            Assert.Equal(double.Epsilon, await DoWorkAsync());
            Assert.Equal(double.Epsilon, await DoWorkAsync(double.Epsilon));
        }

        [Fact]
        public void RunSyncIsOk()
        {
            Assert.Equal(double.Epsilon, AsyncHelpers.RunSync(DoWorkAsync));
            Assert.Equal(double.MaxValue, AsyncHelpers.RunSync(() => DoWorkAsync(double.MaxValue)));
        }

        [Fact]
        public void DoHttpWorkIsOk()
        {
            var mockHandler = new Mock<HttpClientHandler>();
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var client = new HttpClient(mockHandler.Object);
            Assert.True(AsyncHelpers.RunSync(() => client.GetAsync("https://example.com")).IsSuccessStatusCode);
        }

        private static async Task DoWorkException()
        {
            await Task.Run(() => throw new Exception("This is an exception"));
        }

        private async Task<double> DoWorkAsync(double returnValue)
        {
            return await Task.Run(() => returnValue);
        }

        private static async Task<double> DoWorkAsync()
        {
            return await Task.Run(() => double.Epsilon);
        }
    }
}