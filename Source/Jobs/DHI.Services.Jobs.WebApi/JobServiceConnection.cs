namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Jobs;
    using WebApiCore;
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
                var taskRepository = (ITaskRepository<Workflow, string>)Activator.CreateInstance(taskRepositoryType, TaskRepositoryConnectionString.Resolve(), null);
                var taskService = new TaskService<Workflow, string>(taskRepository);
                return new JobService<Workflow, string>(repository, taskService, null);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}