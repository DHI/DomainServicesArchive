namespace DHI.Services.Jobs.Scenarios
{
    using System;
    using DHI.Services;

    [Serializable]
    public class Scenario : BaseEntity<string>
    {
        public Scenario(string id)
            : base(id)
        {
        }

        public Guid? Version { get; set; } = null;

        public Guid? LastJobId { get; set; } = null;

        public DateTime? DateTime { get; set; } = null;

        public string Data { get; set; }

        public DateTime? Deleted { get; set; }
    }
}