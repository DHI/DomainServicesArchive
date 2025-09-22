namespace DHI.Services.Jobs.Automations.TriggerParametersExport
{
    using System.ComponentModel.Composition;
    using DHI.Services.Jobs.Automations.Triggers;

    [Export(typeof(ITriggerParameters))]
    [ExportMetadata("Id", nameof(BluecastTrigger))]
    public class BluecastTriggerParameters : IBluecastTriggerParameters, ITriggerParameters
    {
        [TriggerParameter(true, title: "Database Connection String", description: "The connection string to the database.")]
        public string ConnectionString { get; set; } = "";

        [TriggerParameter(true, title: "File based Connection", description: $"Give path-to-file with connection string if {nameof(ConnectionString)} is not given")]
        public string ConnectionStringFilename { get; set; } = "";

        [TriggerParameter(true, title: "Database Type", description: "The type of database to connect to.")]
        public DbmsType DbmsType { get; set; } = DbmsType.MySQL;

        [TriggerParameter(true, title: "Description")]
        public string Description { get; }

        [TriggerParameter(true, title: "Bulk caching", description: "Use bulk DB caching")]
        public bool UseBulkCaching { get; set; } = true;

        [TriggerParameter(true, title: "Job table")]
        public string JobTable { get; set; } = "Gefs504Cat";

        [TriggerParameter(true, title: "Job type")]
        public string JobType { get; set; } = "Gefs504Cat";

        [TriggerParameter(true, title: "Basetime interval (h)")]
        public string BasetimeIntervalHours { get; set; } = "6";

        [TriggerParameter(true, title: "Block Order")]
        public string BlockOrder { get; set; } = "H06;B1;B2;B3;B4;B5";

        [TriggerParameter(true, title: "Initiate Hours")]
        public string InitiateHours { get; set; } = "-3.25";

        [TriggerParameter(true, title: "Expiry Hours")]
        public string ExpiryHours { get; set; } = "H06=>-120.25;B1=>-48;-18";

        [TriggerParameter(true, title: "Max Run Count")]
        public string MaxRunCount { get; set; } = "H06=>10;3";

        [TriggerParameter(true, title: "Restart Delay Minutes")]
        public string RestartDelayMinutes { get; set; } = "H06=>15;6";

        [TriggerParameter(true, title: "Hot Wait Blocks")]
        public string HotWaitBlocks { get; set; } = "H06";

        [TriggerParameter(true, title: "Chain Wait Blocks")]
        public string ChainWaitBlocks { get; set; } = "B*";

        [TriggerParameter(true, title: "Prerequisite Wait Jobs")]
        public string PrerequisiteWaitJobs { get; set; } = "Gefs504aget[*,H06=>-9.0];Gefs504bget[H06=>-9.0,*]";
    }

    public interface IBluecastTriggerParameters
    {
        string ConnectionString { get; set; }
        string ConnectionStringFilename { get; set; }
        DbmsType DbmsType { get; set; }
    }
}
