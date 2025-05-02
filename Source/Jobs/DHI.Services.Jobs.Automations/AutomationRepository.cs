namespace DHI.Services.Jobs.Automations;

public class AutomationRepository<TTaskId> : GroupedJsonRepository<Automation<TTaskId>>, IAutomationRepository<TTaskId>
{
    public AutomationRepository(string filePath) : base(filePath, AutomationRepositoryConverters.Required)
    {
    }
}

public class AutomationRepository : AutomationRepository<string>, IAutomationRepository
{
    public AutomationRepository(string filePath) : base(filePath)
    {
    }
}