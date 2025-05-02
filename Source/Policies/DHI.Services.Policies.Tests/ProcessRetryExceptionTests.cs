namespace DHI.Services.Policies.Tests
{
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ProcessRetryExceptionTests
    {
        [Fact]
        public async Task SuccessfulExecutionDoesNotRetry()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };
            var policy = new ProcessRetryPolicy(TimeSpan.FromSeconds(1), waitTimes, logger.Object);
            var count = 0;
            using (var process = new Process())
            {
                var proc = await policy.ExecuteAsync(process, () =>
                {
                    process.StartInfo.FileName = @"powershell";

                    process.StartInfo.Arguments = " -command exit $false";

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    count++;
                    process.WaitForExit();
                    return Task.FromResult(process);
                });
                Assert.Equal(0, proc.ExitCode);
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task TestExceptionRetryIsOk()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };
            var policy = new ProcessRetryPolicy(TimeSpan.FromSeconds(1), waitTimes, logger.Object);
            var count = 0;
            using (var process = new Process())
            {
                var proc = await policy.ExecuteAsync(process, () =>
                {
                    process.StartInfo.FileName = @"powershell";

                    process.StartInfo.Arguments = " -command exit $true";

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    count++;
                    process.WaitForExit();

                    return Task.FromResult(process);
                });

                Assert.NotEqual(0, proc.ExitCode);
                Assert.Equal(2, count);
            }
        }

        [Fact]
        public async Task TestTimeRetryIsOk()
        {
            var logger = new Moq.Mock<ILogger>();
            var waitTimes = new TimeSpan[] { TimeSpan.FromSeconds(1) };
            var policy = new ProcessRetryPolicy(TimeSpan.FromSeconds(3), waitTimes, logger.Object);

            var count = 0;
            using (var process = new Process())
            {
                await policy.ExecuteAsync(process, () =>
                {
                    process.StartInfo.FileName = @"powershell";

                    process.StartInfo.Arguments = " -command Start-Sleep -seconds 5";

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    count++;
                    process.WaitForExit();
                    return Task.FromResult(process);
                });
            }

            Assert.Equal(2, count);
        }
    }
}
