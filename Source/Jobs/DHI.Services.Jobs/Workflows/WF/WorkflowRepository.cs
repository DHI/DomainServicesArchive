namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Serialization;
    using Jobs;

    [Obsolete("Use CodeWorkflowRepository instead. This type will eventually be removed.")]
    public class WorkflowRepository : JsonRepository<Workflow, string>, IWorkflowRepository, ITaskRepository<Workflow, string>
    {
        public WorkflowRepository(string filePath, IEnumerable<JsonConverter> converters = null) : base(filePath, converters)
        {
        }

        public new IEnumerable<ITask<string>> GetAll(ClaimsPrincipal user = null)
        {
            var tasks = base.GetAll(user).ToArray();
            foreach (var task in tasks)
            {
                task.Definition = _Upgrade(task.Definition);
            }

            return tasks;
        }

        public new Maybe<ITask<string>> Get(string id, ClaimsPrincipal user = null)
        {
            ITask<string> task = base.Get(id, user) | default(Workflow);
            if (task != null)
            {
                ((Workflow)task).Definition = _Upgrade(((Workflow)task).Definition);
                return task.ToMaybe();
            }

            return Maybe.Empty<ITask<string>>();
        }

        private string _Upgrade(string definition)
        {
            return definition.Replace("DHI.Workflow.Activity.StatusReporting", "DHI.Workflow.Activities.Core").Replace("DHI.Workflow.Activity", "DHI.Workflow.Activities");
        }
    }
}