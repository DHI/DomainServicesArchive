namespace DHI.Services.Jobs.Automations.TriggerParametersExport;

/// <summary>
///     Decorate a trigger export with this interface to make it available for webapi.
/// </summary>
/// <example>
/// <code>
///     [Export(typeof(ITriggerParameters))]
///     [ExportMetadata("Id", nameof(ExampleTrigger))]
///     public class ExampleTriggerParameters : ITriggerParameters
/// </code>
/// </example>
/// <remarks>
///     This is the definition read by the webapi and used to generate the UI.
/// </remarks>
public interface ITriggerParameters
{
    public string Description { get; }
}