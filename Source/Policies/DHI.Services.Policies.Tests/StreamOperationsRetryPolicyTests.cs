namespace DHI.Services.Policies.Tests
{
    using Microsoft.Extensions.Logging;

    public class StreamOperationsRetryPolicyTests
    {
        [Fact]
        public async Task FileAccessPolicyCatchesObjectDisposedException()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new StreamOperationsRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await retry.ExecuteAsync<Task>(() =>
                {
                    i++;
                    throw new ObjectDisposedException("anobject");
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public async Task FileAccessPolicyCatchesFileNotFoundExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new StreamOperationsRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await retry.ExecuteAsync<Task>(() =>
                {
                    i++;
                    throw new InvalidOperationException();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public async Task FileAccessPolicyIgnoresPathExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new StreamOperationsRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await retry.ExecuteAsync<Task>(() =>
                {
                    i++;
                    throw new Exception();
                });
            });

            Assert.Equal(1, i);
        }
    }
}
