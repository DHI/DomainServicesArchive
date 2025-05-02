namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Maps;
    using Xunit;

    public class MapStyleServiceTest
    {
        private const int RepeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new MapStyleService(null));
        }

        [Theory, AutoMapStyleData]
        public void GetNonExistingThrows(MapStyleService mapStyleService)
        {
            Assert.False(mapStyleService.TryGet("UnknownMapStyle", out _));
        }

        [Theory, AutoMapStyleData(RepeatCount)]
        public void GetAllIsOk(MapStyleService mapStyleService)
        {
            Assert.Equal(RepeatCount, mapStyleService.GetAll().Count());
        }

        [Theory, AutoMapStyleData(RepeatCount)]
        public void GetIdsIsOk(MapStyleService mapStyleService)
        {
            Assert.Equal(RepeatCount, mapStyleService.GetIds().Count());
        }

        [Theory, AutoMapStyleData]
        public void AddAndGetIsOk(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            mapStyleService.Add(mapStyle);
            mapStyleService.TryGet(mapStyle.Id, out var jb);
            Assert.Equal(mapStyle.Id, jb.Id);
        }

        [Theory, AutoMapStyleData(RepeatCount)]
        public void CountIsOk(MapStyleService mapStyleService)
        {
            Assert.Equal(RepeatCount, mapStyleService.Count());
        }

        [Theory, AutoMapStyleData(RepeatCount)]
        public void ExistsIsOk(MapStyleService mapStyleService)
        {
            var connection = mapStyleService.GetAll().ToArray()[0];
            Assert.True(mapStyleService.Exists(connection.Id));
        }

        [Theory, AutoMapStyleData(RepeatCount)]
        public void DoesNotExistsIsOk(MapStyleService mapStyleService)
        {
            Assert.False(mapStyleService.Exists("NonExistingConnection"));
        }

        [Theory, AutoMapStyleData]
        public void EventsAreRaisedOnAdd(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            var raisedEvents = new List<string>();
            mapStyleService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            mapStyleService.Added += (s, e) => { raisedEvents.Add("Added"); };

            mapStyleService.Add(mapStyle);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoMapStyleData]
        public void RemoveIsOk(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            mapStyleService.Add(mapStyle);
            mapStyleService.Remove(mapStyle.Id);

            Assert.False(mapStyleService.Exists(mapStyle.Id));
            Assert.Equal(0, mapStyleService.Count());
        }

        [Theory, AutoMapStyleData]
        public void EventsAreRaisedOnRemove(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            var raisedEvents = new List<string>();
            mapStyleService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            mapStyleService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            mapStyleService.Add(mapStyle);

            mapStyleService.Remove(mapStyle.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoMapStyleData]
        public void UpdateIsOk(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            mapStyleService.Add(mapStyle);
            var updatedMapStyle = new MapStyle(mapStyle.Id, "Updated name");
            mapStyleService.Update(updatedMapStyle);

            mapStyleService.TryGet(mapStyle.Id, out var jb);
            Assert.Equal("Updated name", jb.Name);
        }

        [Theory, AutoMapStyleData]
        public void AddOrUpdateIsOk(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            var raisedEvents = new List<string>();
            mapStyleService.Added += (s, e) => { raisedEvents.Add("Added"); };
            mapStyleService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            mapStyleService.AddOrUpdate(mapStyle);
            var updated = new MapStyle(mapStyle.Id, "Updated name");
            mapStyleService.AddOrUpdate(updated);

            mapStyleService.TryGet(mapStyle.Id, out var jb);
            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Name, jb.Name);
        }

        [Theory, AutoMapStyleData]
        public void EventsAreRaisedOnUpdate(MapStyleService mapStyleService, MapStyle mapStyle)
        {
            var raisedEvents = new List<string>();
            mapStyleService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            mapStyleService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            mapStyleService.Add(mapStyle);

            var updatedMapStyle = new MapStyle(mapStyle.Id, "Updated name");
            mapStyleService.Update(updatedMapStyle);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }
    }
}