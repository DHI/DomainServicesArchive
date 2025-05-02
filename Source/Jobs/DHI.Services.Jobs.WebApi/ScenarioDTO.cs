namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Scenarios;

    public class ScenarioDTO
    {
        public string Id { get; set; }

        public Guid? Version { get; set; } = null;

        public Guid? LastJobId { get; set; } = null;

        public DateTime? DateTime { get; set; } = null;

        [Required]
        public string Data { get; set; }

        public Scenario ToScenario()
        {
            var id = !string.IsNullOrEmpty(Id) ? Id : System.DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid();
            return new Scenario(id)
            {
                Version = Version,
                LastJobId = LastJobId,
                DateTime = DateTime,
                Data = Data
            };
        }
    }
}