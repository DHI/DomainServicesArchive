namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CodeWorkflowService : BaseImmutableDiscreteService<CodeWorkflow, string>, ITaskService<CodeWorkflow, string>
    {
        public CodeWorkflowService(ICodeWorkflowRepository repository)
            : base(repository)
        {
        }

        public void ImportFrom(Assembly assembly, bool allowReplace = false)
        {
            var workflowTypes = assembly.GetTypes().Where(t => typeof(ICodeWorkflow).IsAssignableFrom(t)).ToArray();
            foreach (var type in workflowTypes)
            {
                var name = type.GetCustomAttribute<WorkflowNameAttribute>()?.WorkflowName;
                var parameters = new Dictionary<string, object>();
                var parameterFields = GetParameterProperties(type);
                foreach (var (paramName, paramType) in parameterFields)
                {
                    parameters.Add(paramName, paramType.FullName);
                }

                var codeWorkflow = new CodeWorkflow(type.FullName, name ?? type.Name, assembly.GetName().Name)
                {
                    HostGroup = type.GetCustomAttribute<HostGroupAttribute>()?.HostGroup,
                    Timeout = type.GetCustomAttribute<TimeoutAttribute>()?.Timespan,
                    Parameters = parameters
                };

                if (Exists(codeWorkflow.Id) && allowReplace)
                {
                    Remove(codeWorkflow.Id);
                }

                Add(codeWorkflow);
            }
        }

        private static IEnumerable<(string paramName, Type paramType)> GetParameterProperties(Type workflowType)
        {
            foreach (var propertyInfo in workflowType.GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(WorkflowParameterAttribute), true).Length > 0)
                {
                    yield return (propertyInfo.Name, propertyInfo.PropertyType);
                }
            }
        }
    }
}