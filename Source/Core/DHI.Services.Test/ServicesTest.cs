namespace DHI.Services.Test
{
    using System.Collections.Generic;
    using Accounts;
    using Xunit;

    [Collection("ServiceLocator")]
    public class ServicesTest : IClassFixture<ServicesFixture>
    {
        [Fact]
        public void GetForNonExistingConnectionIdThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => Services.Get<AccountService>("NonExistingConnectionId"));
        }

        [Fact]
        public void GetIsOk()
        {
            var myService = Services.Get<FakeService>("MyConnection");
            Assert.IsType<FakeService>(myService);
        }

        [Fact]
        public void AddAndRemoveConnectionIsOk()
        {
            var raisedEvents = new List<string>();
            Services.Connections.Added += (s, e) => { raisedEvents.Add("Added"); };
            Services.Connections.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };

            // Add
            Services.Connections.Add(new FakeConnection("MyNewConnection", "My New Connection"));
            Assert.Equal(2, Services.Connections.Count());
            Assert.IsType<FakeService>(Services.Get<FakeService>("MyNewConnection"));
            Assert.Equal("Added", raisedEvents[0]);

            // Remove
            Services.Connections.Remove("MyNewConnection");
            Assert.Equal(1, Services.Connections.Count());
            Assert.Throws<KeyNotFoundException>(() => Services.Get<FakeService>("MyNewConnection"));
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Fact]
        public void UpdateConnectionIsOk()
        {
            var raisedEvents = new List<string>();
            Services.Connections.Added += (s, e) => { raisedEvents.Add("Added"); };
            Services.Connections.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            Services.Connections.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };

            var myNewConnection = new FakeConnection("MyNewConnection", "My New Connection");
            // Add
            Services.Connections.Add(myNewConnection);
            Assert.Equal(2, Services.Connections.Count());
            Assert.IsType<FakeService>(Services.Get<FakeService>("MyNewConnection"));
            Assert.Equal("Added", raisedEvents[0]);

            // Update
            myNewConnection = new FakeConnection("MyNewConnection", "My Updated Connection");
            Services.Connections.Update(myNewConnection);
            Services.Connections.TryGet(myNewConnection.Id, out var myUpdatedConnection);
            Assert.Equal(myNewConnection.Name, myUpdatedConnection.Name);
            Assert.Equal(2, Services.Connections.Count());
            Assert.Equal("Updated", raisedEvents[1]);

            // Remove
            Services.Connections.Remove("MyNewConnection");
            Assert.Equal(1, Services.Connections.Count());
            Assert.Throws<KeyNotFoundException>(() => Services.Get<FakeService>("MyNewConnection"));
            Assert.Equal("Deleted", raisedEvents[2]);
        }
    }
}