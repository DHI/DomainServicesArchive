namespace DHI.Services.Jobs.WorkflowWorker.Test
{
    using Moq;
    using System;
    using System.Security.Claims;
    using Xunit;

    public class HostServiceTests
    {
        [Fact]
        public void UnimplementedMethodsThrow()
        {
            var hostCollection = new Mock<ISignalRHostCollection>();
            var signalRHostService = new SignalRHostService(hostCollection.Object, new string[0]);
            Assert.Throws<NotImplementedException>(() => signalRHostService.Add(null));
            Assert.Throws<NotImplementedException>(() => signalRHostService.AddOrUpdate(null));
            Assert.Throws<NotImplementedException>(() => signalRHostService.AdjustJobCapacity(0));
            Assert.Throws<NotImplementedException>(() => signalRHostService.CreateHost(null));
            Assert.Throws<NotImplementedException>(() => signalRHostService.Remove(null));
            Assert.Throws<NotImplementedException>(() => signalRHostService.TryAdd(null));
            Assert.Throws<NotImplementedException>(() => signalRHostService.TryUpdate(null));
            Assert.Throws<NotImplementedException>(() => signalRHostService.Update(null));
        }

        [Fact]
        public void ExistsIsOk()
        {
            var hostCollection = new Mock<ISignalRHostCollection>();
            hostCollection.Setup(a => a.Contains("one", It.IsAny<ClaimsPrincipal>())).Returns(true);
            hostCollection.Setup(a => a.Contains("two", It.IsAny<ClaimsPrincipal>())).Returns(false);
            var signalRHostService = new SignalRHostService(hostCollection.Object, new string[0]);
            Assert.True(signalRHostService.Exists("one"));
            Assert.False(signalRHostService.Exists("two"));
        }

        [Fact]
        public void GetIsOk()
        {
            var hostCollection = new Mock<ISignalRHostCollection>();
            hostCollection.Setup(a => a.Get("one", It.IsAny<ClaimsPrincipal>())).Returns(() => (new Host("one", "one")).ToMaybe());
            hostCollection.Setup(a => a.GetAll(It.IsAny<ClaimsPrincipal>())).Returns(() => new Host[] { new Host("one", "one"), new Host("two", "two") });

            var signalRHostService = new SignalRHostService(hostCollection.Object, new string[0]);

            Assert.Collection(signalRHostService.GetAll(),
               one => Assert.Equal("one", one.Id),
               two => Assert.Equal("two", two.Id));

        }

        [Fact]
        public void GetHostsIsOk()
        {
            var hostCollection = new Mock<ISignalRHostCollection>();
            hostCollection.Setup(a => a.GetGroupMembers("A")).Returns(() => new Host[] { new Host("one", "one"), new Host("two", "two") { } });
            hostCollection.Setup(a => a.GetGroupMembers("B")).Returns(() => new Host[] { new Host("three", "three"), new Host("four", "four") });
            hostCollection.Setup(a => a.GetAll(It.IsAny<ClaimsPrincipal>())).Returns(() => new Host[] { new Host("one", "one"), new Host("two", "two") });
            var signalRHostService = new SignalRHostService(hostCollection.Object, new string[0]);

            Assert.Collection(signalRHostService.GetByGroups(new string[] { "A", "B" }),
                one => Assert.Equal("one", one.Id),
                two => Assert.Equal("two", two.Id),
                three => Assert.Equal("three", three.Id),
                four => Assert.Equal("four", four.Id));

            Assert.Collection(signalRHostService.GetFullNames(),
                one => Assert.Equal("one", one),
                two => Assert.Equal("two", two));

            Assert.Collection(signalRHostService.GetFullNames("A"),
                one => Assert.Equal("one", one),
                two => Assert.Equal("two", two));

            Assert.Collection(signalRHostService.GetIds(),
                one => Assert.Equal("one", one),
                two => Assert.Equal("two", two));
        }

        [Fact]
        public void GroupExistsIsOk()
        {
            var hostCollection = new Mock<ISignalRHostCollection>();
            var signalRHostService = new SignalRHostService(hostCollection.Object, new string[] { "test" });

            Assert.True(signalRHostService.GroupExists("test"));
            Assert.False(signalRHostService.GroupExists("nope"));
        }
    }
}
