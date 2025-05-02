namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Maps;
    using Xunit;

    public class GroupedMapServiceTest : IClassFixture<GroupedMapSourceFixture>
    {
        private readonly GroupedMapService _service;

        public GroupedMapServiceTest(GroupedMapSourceFixture fixture)
        {
            _service = new GroupedMapService(fixture.GroupedMapSource);
        }

        [Fact]
        public void CreateWithNullMapSourceThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedMapService(null));
        }

        [Fact]
        public void GetByNonExistingGroupThrows()
        {
            var e = Assert.Throws<KeyNotFoundException>(() => _service.GetByGroup("nonExistingGroup"));
            Assert.Contains("does not exist", e.Message);
        }

        [Fact]
        public void GetFullNamesForNonExistingGroupThrows()
        {
            var e = Assert.Throws<KeyNotFoundException>(() => _service.GetFullNames("nonExistingGroup"));
            Assert.Contains("does not exist", e.Message);
        }

        [Fact]
        public void GetNonExistingLayerThrows()
        {
            Assert.False(_service.TryGet("nonExistingLayer", out _));
        }

        [Fact]
        public void GetByGroupIsOk()
        {
            var layers = _service.GetByGroup("Riverbank gardens");
            Assert.Equal(2, layers.Count());
        }

        [Fact]
        public void GetByMultipleGroupsIsOk()
        {
            Assert.Equal(3, _service.GetByGroups(new List<string> { "Riverbank gardens", "Westbank gardens" }).Count());
        }

        [Fact]
        public void GetFullNamesIsOk()
        {
            var fullNames = _service.GetFullNames();
            Assert.Equal(3, fullNames.Count());
        }

        [Fact]
        public void GetFullNamesByGroupIsOk()
        {
            var fullNames = _service.GetFullNames("Riverbank gardens");
            Assert.Equal(2, fullNames.Count());

            fullNames = _service.GetFullNames("Westbank gardens");
            Assert.Single(fullNames);
        }

        [Fact]
        public void GetIsOk()
        {
            _service.TryGet("Riverbank gardens/CDZ_rivergarden_2016-2018", out var layer );
            Assert.Equal("Riverbank gardens/CDZ_rivergarden_2016-2018", layer.FullName);
        }

        [Fact]
        public void GetMultipleIsOk()
        {
            _service.TryGet(new[] { "Riverbank gardens/CDZ_rivergarden_2016-2018", "Westbank gardens/CDZ_westgarden" }, out var layers );
            Assert.Equal(2, layers.Count());
        }

        [Fact]
        public void GetAllIsOk()
        {
            Assert.Equal(3, _service.GetAll().Count());
        }

        [Fact]
        public void GetIdsIsOk()
        {
            var ids = _service.GetIds().ToArray();
            Assert.Equal(3, ids.Length);
            Assert.Contains("Westbank gardens/CDZ_westgarden", ids);
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.Equal(3, _service.Count());
        }

        [Fact]
        public void ExistsIsOk()
        {
            Assert.True(_service.Exists("Riverbank gardens/CDZ_rivergarden_2016-2018"));
        }

        [Fact]
        public void DoesNotExistsIsOk()
        {
            Assert.False(_service.Exists("NonExistingEntity"));
        }
    }
}