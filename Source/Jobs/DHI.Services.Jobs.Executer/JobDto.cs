namespace DHI.Services.Jobs.Executer
{
    using System.Collections.Generic;

    public class JobDto
    {
        public JobDto(string taskId)
        {
            TaskId = taskId;
        }

        public string TaskId { get; set; }

        public string? HostGroup { get; set; }

        public int? Priority { get; set; }
        
        public string? Tag { get; set; }

        public Dictionary<string, object>? Parameters { get; set; }
    }
}