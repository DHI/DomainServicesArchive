namespace DHI.Services.Jobs.Automations.TriggerParametersExport;

using System.ComponentModel.Composition;
using Triggers;

[Export(typeof(ITriggerParameters))]
[ExportMetadata("Id", nameof(SqlTrigger))]
public class SqlTriggerParameters : ISqlTriggerParameters, ITriggerParameters
{
    [TriggerParameter(true, title: "The queries to run", description: "A list of sql queries to be run to sequence, the results of a query will be feed into the next")]
    public string[] Queries { get; set; }

    [TriggerParameter(true, title: "Database Connection String", description: "The connection string to the database.")]
    public string ConnectionString { get; set; }

    [TriggerParameter(true, title: "Database Type", description: "The type of database to connect to.")]
    public DbmsType DbmsType { get; set; }

    [TriggerParameter(true, title: "Description")]
    public string Description { get; }
}

public interface ISqlTriggerParameters
{
    string[] Queries { get; set; }
    string ConnectionString { get; set; }
    DbmsType DbmsType { get; set; }
}