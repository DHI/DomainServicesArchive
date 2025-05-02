namespace DHI.Services.Jobs.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Accounts;
    using Jobs;
    using Workflows;

    /// <summary>
    ///     JobServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class JobServiceConnection : JobServiceConnection<Workflow, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public JobServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a JobService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var jobRepositoryType = Type.GetType(JobRepositoryType, true);
                var repository = (IJobRepository<Guid, string>)Activator.CreateInstance(jobRepositoryType, JobRepositoryConnectionString);
                var taskRepositoryType = Type.GetType(TaskRepositoryType, true);
                var taskRepository = (ITaskRepository<Workflow, string>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString.Resolve());
                var taskService = new TaskService<Workflow, string>(taskRepository);
                var accountService = ServiceLocator.Get<AccountService>(ServiceId.Accounts);
                return new JobService<Workflow, string>(repository, taskService, accountService);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}