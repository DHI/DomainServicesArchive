namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Jobs;
    using Scenarios;
    using WebApiCore;
    using Workflows;

    /// <summary>
    ///     ScenarioServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class ScenarioServiceConnection : ScenarioServiceConnection<Workflow, string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScenarioServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public ScenarioServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates an ScenarioService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (IScenarioRepository)Activator.CreateInstance(repositoryType, ConnectionString.Resolve(), null);

                if (JobRepositoryType != string.Empty && JobRepositoryConnectionString != null)
                {
                    var jobRepositoryType = Type.GetType(JobRepositoryType, true);
                    var jobRepository = (IJobRepository<Guid, string>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString.Resolve());
                    return new ScenarioService(repository, jobRepository);
                }

                return new ScenarioService(repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}