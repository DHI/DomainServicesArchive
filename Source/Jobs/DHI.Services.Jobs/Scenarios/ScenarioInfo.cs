namespace DHI.Services.Jobs.Scenarios
{
    using System.Text.Json.Serialization;

    public class ScenarioInfo : Scenario
    {
        [JsonConstructor]
        public ScenarioInfo(string id) : base(id)
        {
        }

        public ScenarioInfo(Scenario scenario) : this(scenario.Id)
        {
            Version = scenario.Version;
            LastJobId = scenario.LastJobId;
            DateTime = scenario.DateTime;
            Data = scenario.Data;
            Deleted = scenario.Deleted;
        }

        public JobStatus LastJobStatus { get; set; } = JobStatus.Unknown;

        public int? LastJobProgress { get; set; } = null;
    }
}