namespace DHI.Services.Policies.Tests
{
    using Microsoft.Extensions.Logging;

    public class ExceptionRetryPolicyTests
    {
        [Fact]
        public void ExceptionRetryPolicyCatchesMatchingException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(Exception) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new ExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<Exception>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new Exception();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public void ExceptionRetryPolicyIgnoresUnmatchingException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(Exception) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new ExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<InvalidOperationException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new InvalidOperationException();
                });
            });

            Assert.Equal(1, i);
        }


        [Fact]
        public void ExceptionRetryPolicyCatchesDerivedException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(InvalidOperationException) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new ExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<InvalidOperationException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new InvalidOperationException();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public void ExceptionRetryPolicyIgnoresUnmatchingParentException()
        {
            var logger = new Moq.Mock<ILogger>();
            var exceptionTypes = new[] { typeof(InvalidOperationException) };
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new ExceptionRetryPolicy(exceptionTypes, waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<Exception>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new Exception();
                });
            });

            Assert.Equal(1, i);
        }
    }
}