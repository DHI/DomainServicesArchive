namespace DHI.Services.Jobs.Web
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Workflows;

    /// <summary>
    ///     TaskServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class TaskServiceConnection : TaskServiceConnection<Workflow, string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public TaskServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a TaskService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new TaskService<Workflow, string>((ITaskRepository<Workflow, string>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}