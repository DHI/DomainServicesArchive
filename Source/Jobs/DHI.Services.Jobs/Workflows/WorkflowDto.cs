using System;
using System.Collections.Generic;
using System.IO;

namespace DHI.Services.Jobs.Workflows
{
    public interface IWorkflowDto<TDefinition> : IWorkflowDto
    {
        TDefinition Definition { get; set; }
    }

    public interface IWorkflowDto
    {
        string HostId { get; set; }
        Guid JobId { get; set; }
        Dictionary<string, object> Parameters { get; set; }

        void SaveToFile(StreamWriter streamWriter);
    }

    public class WorkflowDto : IWorkflowDto
    {
        public Guid JobId { get; set; }
        public string HostId { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public virtual void SaveToFile(StreamWriter streamWriter)
        {
            throw new NotImplementedException("Implement in derived class");
        }
    }
}