namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class CodeWorkflow : BaseNamedEntity<string>, ITask<string>
    {
        public CodeWorkflow(string id, string name, string assemblyName) : base(id, name)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }

        public TimeSpan? Timeout { get; set; }

        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public string HostGroup { get; set; }

        public bool ShouldSerializeParameters()
        {
            return Parameters?.Count > 0;
        }

        public CodeWorkflowDefinition ToDefinition()
        {
            return new CodeWorkflowDefinition { AssemblyName = AssemblyName, TypeName = Id };
        }
    }
}