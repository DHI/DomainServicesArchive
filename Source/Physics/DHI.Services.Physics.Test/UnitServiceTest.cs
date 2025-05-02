namespace DHI.Services.Physics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Physics;
    using Physics;
    using Xunit;
    using Unit = Unit;

    public class UnitServiceTest
    {
        private const int RepeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new UnitService(null));
        }

        [Theory, AutoUnitData]
        public void GetNonExistingThrows(UnitService unitService)
        {
            Assert.Throws<KeyNotFoundException>(() => unitService.Get("UnknownUnit"));
        }

        [Theory, AutoUnitData]
        public void UpdateNonExistingThrows(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            Assert.Throws<KeyNotFoundException>(() => unitService.Update(unit));
        }

        [Theory, AutoUnitData]
        public void RemoveNonExistingThrows(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            Assert.Throws<KeyNotFoundException>(() => unitService.Remove(unit.Id));
        }

        [Theory, AutoUnitData(RepeatCount)]
        public void GetAllIsOk(UnitService unitService)
        {
            Assert.Equal(RepeatCount, unitService.GetAll().Count());
        }

        [Theory, AutoUnitData(RepeatCount)]
        public void GetIdsIsOk(UnitService unitService)
        {
            Assert.Equal(RepeatCount, unitService.GetIds().Count());
        }

        [Theory, AutoUnitData]
        public void AddAndGetIsOk(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            unitService.Add(unit);
            Assert.Equal(unit.Id, unitService.Get(unit.Id).Id);
        }

        [Theory, AutoUnitData(RepeatCount)]
        public void CountIsOk(UnitService unitService)
        {
            Assert.Equal(RepeatCount, unitService.Count());
        }

        [Theory, AutoUnitData(RepeatCount)]
        public void ExistsIsOk(UnitService unitService)
        {
            var unit = unitService.GetAll().ToArray()[0];
            Assert.True(unitService.Exists(unit.Id));
        }

        [Theory, AutoUnitData(RepeatCount)]
        public void DoesNotExistsIsOk(UnitService unitService)
        {
            Assert.False(unitService.Exists("NonExistingUnit"));
        }

        [Theory, AutoUnitData]
        public void EventsAreRaisedOnAdd(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            var raisedEvents = new List<string>();
            unitService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            unitService.Added += (s, e) => { raisedEvents.Add("Added"); };

            unitService.Add(unit);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoUnitData]
        public void RemoveIsOk(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            unitService.Add(unit);
            unitService.Remove(unit.Id);

            Assert.False(unitService.Exists(unit.Id));
            Assert.Equal(0, unitService.Count());
        }

        [Theory, AutoUnitData]
        public void EventsAreRaisedOnRemove(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            var raisedEvents = new List<string>();
            unitService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            unitService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            unitService.Add(unit);

            unitService.Remove(unit.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoUnitData]
        public void UpdateIsOk(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            unitService.Add(unit);

            var updated = new Unit(unit.Id, "Updated description", unit.Abbreviation, unit.Factor, unit.Dimension);
            unitService.Update(updated);

            Assert.Equal("Updated description", unitService.Get(unit.Id).Description);
        }

        [Theory, AutoUnitData]
        public void AddOrUpdateIsOk(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            var raisedEvents = new List<string>();
            unitService.Added += (s, e) => { raisedEvents.Add("Added"); };
            unitService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            unitService.AddOrUpdate(unit);
            var updated = new Unit(unit.Id, "Updated description", unit.Abbreviation, unit.Factor, unit.Dimension);
            unitService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            Assert.Equal(updated.Description, unitService.Get(unit.Id).Description);
        }

        [Theory, AutoUnitData]
        public void EventsAreRaisedOnUpdate(UnitService unitService)
        {
            var unit = new Unit("meter", "meter", "m", 1, Dimension.Length);
            var raisedEvents = new List<string>();
            unitService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            unitService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            unitService.Add(unit);

            var updated = new Unit(unit.Id, "Updated description", unit.Abbreviation, unit.Factor, unit.Dimension);
            unitService.Update(updated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }

        [Theory, AutoUnitData]
        public void RegisterConversionIsOk(UnitService unitService)
        {
            unitService.Add(new Unit("fahrenheit", "fahrenheit", "F", 1, Dimension.Temperature));
            unitService.Add(new Unit("celcius", "celcius", "C", 1, Dimension.Temperature));
            unitService.RegisterConversion("fahrenheit", "celcius", d => (d - 32.0) * 5.0 / 9.0);

            Assert.Equal(20, unitService.Convert(68, "fahrenheit", "celcius"));
        }
    }
}