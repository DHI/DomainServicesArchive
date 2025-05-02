namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class HostTest
    {
        [Fact]
        public void IsCloudInstanceIsOk()
        {
            var host = new Host("myHost", "My Host")
            {
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = { { "HostId", Guid.NewGuid().ToString() } }
            };

            Assert.True(host.IsCloudInstance());
        }

        [Fact]
        public void IsNotCloudInstanceIsOk()
        {
            var host = new Host("myHost", "My Host");

            Assert.False(host.IsCloudInstance());
        }

        [Fact]
        public void CloudInstanceHandlerGetStatusIsOk()
        {
            var host = new Host("myHost", "My Host")
            {
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = { { "HostId", Guid.NewGuid().ToString() } }
            };

            Assert.Equal(CloudInstanceStatus.Stopped, host.CloudInstanceHandler.GetStatus());
        }

        [Fact]
        public void CloudInstanceHandlerStartIsOk()
        {
            var host = new Host("myHost", "My Host")
            {
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = { { "HostId", Guid.NewGuid().ToString() } }
            };

            host.CloudInstanceHandler.Start();
            Assert.Equal(CloudInstanceStatus.Starting, host.CloudInstanceHandler.GetStatus());
        }

        [Fact]
        public async Task CloudInstanceHandlerStartAndStopIsOk()
        {
            var host = new Host("myHost", "My Host")
            {
                CloudInstanceHandlerType = "DHI.Services.Jobs.Test.FakeCloudInstanceHandler, DHI.Services.Jobs.Test",
                CloudInstanceParameters = { { "HostId", Guid.NewGuid().ToString() } }
            };

            await host.CloudInstanceHandler.Start();
            Assert.Equal(CloudInstanceStatus.Running, host.CloudInstanceHandler.GetStatus());
            await host.CloudInstanceHandler.Stop();
            Assert.Equal(CloudInstanceStatus.Stopped, host.CloudInstanceHandler.GetStatus());
        }
    }
}