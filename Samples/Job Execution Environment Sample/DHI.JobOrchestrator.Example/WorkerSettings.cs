
namespace DHI.JobOrchestratorService.Settings
{
    public class WorkerSettings
    {
        public bool Enabled { get; set; } = true;
        public string? HostGroup { get; set; }
        public string JobRepositoryConnectionString { get; set; } = "";

        public string Environment { get; set; } = "";

        public string EnvironmentQualifiedName(string name)
        {
            return $"{name}{Environment}";
        }
    }
}
