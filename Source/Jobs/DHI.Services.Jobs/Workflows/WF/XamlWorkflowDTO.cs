namespace DHI.Services.Jobs.Workflows.WF
{
    using System.IO;

    public class XamlWorkflowDTO : WorkflowDto, IWorkflowDto<string>
    {
        public string Definition { get; set; }

        public override void SaveToFile(StreamWriter streamWriter)
        {
            var dtoFileContent = System.Text.Json.JsonSerializer.Serialize(this);
            streamWriter.WriteLine(dtoFileContent);
            streamWriter.Flush();
        }
    }
}