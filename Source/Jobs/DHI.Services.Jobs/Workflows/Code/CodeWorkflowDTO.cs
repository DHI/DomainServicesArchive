namespace DHI.Services.Jobs.Workflows.Code
{
    using System.IO;

    public class CodeWorkflowDTO : WorkflowDto, IWorkflowDto<CodeWorkflowDefinition>
    {
        public CodeWorkflowDefinition Definition { get; set; }

        public override void SaveToFile(StreamWriter streamWriter)
        {
            var dtoFileContent = System.Text.Json.JsonSerializer.Serialize(this);
            streamWriter.Write(dtoFileContent);
            streamWriter.Flush();
        }
    }
}