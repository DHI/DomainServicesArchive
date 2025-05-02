namespace JobOrchestratorWinService
{
    public class WorkerSettings
    {
        /// <summary>
        /// The hostgroup that this worker belongs to.
        /// </summary>
        public string? HostGroup { get; set; }

        /// <summary>
        /// The connection string to the job repository.
        /// </summary>
        public string JobRepositoryConnectionString { get; set; } = "";

        /// <summary>
        /// The connection string to the workflow repository.
        /// </summary>
        public string WorkflowRepositoryConnectionString { get; set; } = "";

        /// <summary>
        /// The connection string to the host repository.
        /// </summary>
        public string HostRepositoryConnectionString { get; set; } = "";
    }
}
