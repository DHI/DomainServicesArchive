namespace DHI.Services.Policies.Tests
{
    using Microsoft.Extensions.Logging;

    public class FileAccessRetryPolicyTests
    {
        [Fact]
        public void FileAccessPolicyCatchesIOExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new FileAccessRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<IOException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new IOException();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public void FileAccessPolicyCatchesFileNotFoundExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new FileAccessRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<FileNotFoundException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new FileNotFoundException();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public void FileAccessPolicyCatchesDirectoryNotFoundExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new FileAccessRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new DirectoryNotFoundException();
                });
            });

            Assert.Equal(2, i);
        }

        [Fact]
        public void FileAccessPolicyIgnoresAccessExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new FileAccessRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new UnauthorizedAccessException();
                });
            });

            Assert.Equal(1, i);
        }

        [Fact]
        public void FileAccessPolicyIgnoresPathExceptions()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };

            var retry = new FileAccessRetryPolicy(waitTimes, logger.Object);
            var i = 0;

            Assert.Throws<PathTooLongException>(() =>
            {
                retry.Execute(() =>
                {
                    i++;
                    throw new PathTooLongException();
                });
            });

            Assert.Equal(1, i);
        }
    }
}
