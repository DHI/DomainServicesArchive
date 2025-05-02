namespace DHI.Services.Models.Test
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class ScenarioTest
    {
        [Fact]
        public void CreateWithNullOrEmptyModelIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Scenario("myScenario", "My Scenario", null!));
            Assert.Throws<ArgumentException>(() => new Scenario("myScenario", "My Scenario", ""));
        }

        [Fact]
        public void CreateIsOk()
        {
            var scenario = new Scenario(
                "myScenario",
                "My Scenario",
                "myModelDataReader",
                new Dictionary<string, object> {{"someParam", "foo"}},
                new Dictionary<string, string> { {"tsId", "telemetry/wl/rainstation/catchment1"}},
                new Dictionary<object, object> {{"someMetadata", 99}});

            Assert.Contains("someParam", scenario.ParameterValues);
            Assert.Contains("tsId", scenario.InputTimeSeriesValues);
            Assert.Contains("someMetadata", scenario.Metadata);
        }
    }
}