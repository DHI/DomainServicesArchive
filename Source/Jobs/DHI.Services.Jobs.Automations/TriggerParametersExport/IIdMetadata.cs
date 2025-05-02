namespace DHI.Services.Jobs.Automations.TriggerParametersExport;

/// <summary>
///     Used to label an exports metadata with an Id.
/// </summary>
public interface IIdMetadata
{
    public string Id { get; }
}