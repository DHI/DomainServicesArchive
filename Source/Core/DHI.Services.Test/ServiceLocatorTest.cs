namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    [Collection("ServiceLocator")]
    public class ServiceLocatorTest
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "")]
        public void RegisterNullOrEmptyStringThrows(object service, string serviceId)
        {
            Assert.Throws<ArgumentNullException>(() => ServiceLocator.Register(service, serviceId));
        }

        [Fact]
        public void GetNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceLocator.Get<FakeService>(null));
        }

        [Fact]
        public void GetEmptyStringThrows()
        {
            Assert.Throws<ArgumentException>(() => ServiceLocator.Get<FakeService>(""));
        }

        [Fact]
        public void GetNonExistingThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => ServiceLocator.Get<FakeService>("NonExistingService"));
        }

        [Fact]
        public void RemoveNullOrEmptyStringThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceLocator.Remove(null));
            Assert.Throws<ArgumentException>(() => ServiceLocator.Remove(""));
        }

        [Fact]
        public void RemoveNonExistingThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => ServiceLocator.Remove("NonExistingService"));
        }

        [Fact]
        public void DoesNotContainIsOk()
        {
            Assert.False(ServiceLocator.Contains("NonExistingService"));
        }

        [Theory]
        [InlineData("MyService")]
        public void CRUDIsOk(string serviceId)
        {
            ServiceLocator.Register(new FakeService(), serviceId);
            Assert.True(ServiceLocator.Contains(serviceId));
            Assert.IsType<FakeService>(ServiceLocator.Get<FakeService>(serviceId));
            ServiceLocator.Remove(serviceId);
            Assert.False(ServiceLocator.Contains(serviceId));
        }

        [Fact]
        public void GetAllIsOk()
        {
            ServiceLocator.Register(new FakeService(), "MyService1");
            ServiceLocator.Register(new FakeService(), "MyService2");
            Assert.True(ServiceLocator.Contains("MyService1"));
            Assert.True(ServiceLocator.Contains("MyService2"));
            ServiceLocator.Remove("MyService1");
            ServiceLocator.Remove("MyService2");
            Assert.False(ServiceLocator.Contains("MyService1"));
            Assert.False(ServiceLocator.Contains("MyService2"));
        }

        [Fact]
        public void GetTypesIsOk()
        {
            ServiceLocator.Register(new FakeService(), "MyService1");
            ServiceLocator.Register(new FakeService(), "MyService2");
            var types = ServiceLocator.GetTypes();
            Assert.Contains("MyService1", types.Keys);
            Assert.Equal(typeof(FakeService), types["MyService1"]);
            Assert.Contains("MyService2", types.Keys);
            Assert.Equal(typeof(FakeService), types["MyService2"]);
            ServiceLocator.Remove("MyService1");
            ServiceLocator.Remove("MyService2");
        }

        [Fact]
        public void CountIsOk()
        {
            ServiceLocator.Register(new FakeService(), "MyService1");
            ServiceLocator.Register(new FakeService(), "MyService2");
            Assert.Equal(2, ServiceLocator.Count);
            ServiceLocator.Remove("MyService1");
            ServiceLocator.Remove("MyService2");
            Assert.Equal(0, ServiceLocator.Count);
        }

        [Fact]
        public void IdsIsOk()
        {
            ServiceLocator.Register(new FakeService(), "MyService1");
            ServiceLocator.Register(new FakeService(), "MyService2");
            Assert.Contains("MyService1", ServiceLocator.Ids);
            Assert.Contains("MyService2", ServiceLocator.Ids);
            ServiceLocator.Remove("MyService1");
            ServiceLocator.Remove("MyService2");
            Assert.Empty(ServiceLocator.Ids);
        }
    }
}