namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Rasters;
    using Xunit;
    using Zones;

    public class ZoneServiceTest
    {
        private readonly IFixture _fixture;

        public ZoneServiceTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Register<IZoneRepository>(() => new FakeZoneRepository());
        }

        [Fact]
        public void CreateWithNullRepositoryWillThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneService(null));
        }

        [Fact]
        public void GetNonExistingThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            Assert.False(zones.TryGet("nonExistingId", out _));
        }

        [Fact]
        public void RemoveNonExistingThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            Assert.Throws<KeyNotFoundException>(() => zones.Remove("nonExistingId"));
        }

        [Fact]
        public void UpdateNonExistingThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new Zone("NonExistingZone", "ZoneName");

            Assert.Throws<KeyNotFoundException>(() => zones.Update(zone));
        }

        [Fact]
        public void AddExistingThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new FakeZone();
            zones.Add(zone);

            Assert.Throws<ArgumentException>(() => zones.Add(zone));
        }

        [Fact]
        public void AddWithExistingNameThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new FakeZone();
            zones.Add(zone);
            var zoneWithSameName = new Zone("Zone2", zone.Name);

            Assert.Throws<ArgumentException>(() => zones.Add(zoneWithSameName));
        }

        [Fact]
        public void AddWithNoPixelWeightsThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new Zone("MyZone", "MyZoneName");

            Assert.Throws<ArgumentException>(() => zones.Add(zone));
        }

        [Fact]
        public void AddWithIllegalPixelWeightsThrows()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new Zone("MyZone", "MyZoneName");
            zone.PixelWeights.Add(new PixelWeight(new Pixel(10, 10), new Weight(0.6)));
            zone.PixelWeights.Add(new PixelWeight(new Pixel(10, 10), new Weight(0.7)));

            Assert.Throws<ArgumentException>(() => zones.Add(zone));
        }

        [Fact]
        public void AddAndGetIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new FakeZone();
            zones.Add(zone);

            zones.TryGet(zone.Id, out var rs);
            Assert.Equal(zone.Id, rs.Id);
        }

        [Fact]
        public void EventsAreRaisedOnAdd()
        {
            var raisedEvents = new List<string>();
            var zones = _fixture.Create<ZoneService>();
            zones.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            zones.Added += (s, e) => { raisedEvents.Add("Added"); };
            var zone = new FakeZone();

            zones.Add(zone);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Fact]
        public void GetAllIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone1 = new FakeZone("Zone1", "Zone1Name");
            var zone2 = new FakeZone("Zone2", "Zone2Name");
            zones.Add(zone1);
            zones.Add(zone2);

            Assert.Equal(2, zones.GetAll().Count());
        }

        [Fact]
        public void CountIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone1 = new FakeZone("Zone1", "Zone1Name");
            var zone2 = new FakeZone("Zone2", "Zone2Name");
            zones.Add(zone1);
            zones.Add(zone2);

            Assert.Equal(2, zones.Count());
        }

        [Fact]
        public void ExistsIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new FakeZone();
            zones.Add(zone);

            Assert.True(zones.Exists(zone.Id));
        }

        [Fact]
        public void DoesNotExistIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            Assert.False(zones.Exists("NonExistingId"));
        }

        [Fact]
        public void RemoveIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new FakeZone();
            zones.Add(zone);

            zones.Remove(zone.Id);

            Assert.False(zones.Exists(zone.Id));
        }

        [Fact]
        public void EventsAreRaisedOnRemove()
        {
            var raisedEvents = new List<string>();
            var zones = _fixture.Create<ZoneService>();
            zones.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            zones.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            var zone = new FakeZone();
            zones.Add(zone);

            zones.Remove(zone.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Fact]
        public void UpdateIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var zone = new FakeZone("MyZone", "My zone name");
            zones.Add(zone);

            var zoneUpdated = new FakeZone("MyZone", "My updated zone name");
            zones.Update(zoneUpdated);

            zones.TryGet(zone.Id, out var rs);
            Assert.Equal(zoneUpdated.Name, rs.Name);
        }

        [Fact]
        public void AddOrUpdateIsOk()
        {
            var zones = _fixture.Create<ZoneService>();
            var raisedEvents = new List<string>();
            zones.Added += (s, e) => { raisedEvents.Add("Added"); };
            zones.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            var zone = new FakeZone("MyZone", "My zone name");
            zones.Add(zone);
            var updated = new FakeZone("MyZone", "My updated zone name");
            zones.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            zones.TryGet(zone.Id, out var rs);
            Assert.Equal(updated.Name, rs.Name);
        }

        [Fact]
        public void EventsAreRaisedOnUpdate()
        {
            var raisedEvents = new List<string>();
            var zones = _fixture.Create<ZoneService>();
            zones.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            zones.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            var zone = new FakeZone("MyZone", "My zone name");
            zones.Add(zone);

            var zoneUpdated = new FakeZone("MyZone", "My updated zone name");
            zones.Update(zoneUpdated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Fact]
        public void GetRepositoryTypesExplicitPathIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = ZoneService.GetRepositoryTypes(path);

            Assert.Contains(typeof(ZoneRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesExplicitSearchPatternIsOk()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = ZoneService.GetRepositoryTypes(path, "DHI.Services*.dll");

            Assert.Contains(typeof(ZoneRepository), repositoryTypes);
        }

        [Fact]
        public void GetRepositoryTypesWrongSearchPatternReturnsEmpty()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var repositoryTypes = ZoneService.GetRepositoryTypes(path, "DHI.Solutions*.dll");

            Assert.Empty(repositoryTypes);
        }
    }
}