namespace DHI.Services.Policies.Tests
{
    using Microsoft.Extensions.Logging;

    public class AsyncExceptionRetryPolicyTests
    {
        [Fact]
        public async Task AsyncExceptionRetryPolicyCatchesMatchingException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(Exception) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new AsyncExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
            var i = 0;

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await retry.ExecuteAsync<Task>(() =>
                {
                    i++;
                    throw new Exception();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public async Task AsyncExceptionRetryPolicyIgnoresUnmatchingException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(Exception) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new AsyncExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
            var i = 0;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await retry.ExecuteAsync<Task>(() =>
                {
                    i++;
                    throw new InvalidOperationException();
                });
            });

            Assert.Equal(1, i);
        }


        [Fact]
        public async Task AsyncExceptionRetryPolicyCatchesDerivedException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(InvalidOperationException) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new AsyncExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
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
        public async Task AsyncExceptionRetryPolicyIgnoresUnmatchingParentException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(InvalidOperationException) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new AsyncExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
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