namespace DHI.Services.Jobs.Test
{
    using System;
    using System.IO;
    using Workflows;
    using Xunit;

    public class WorkflowTest
    {
        [Theory, AutoMoqData]
        public void WrongFileExtensionInSaveAsXamlThrows(Workflow workflow)
        {
            Assert.Throws<ArgumentException>(() => workflow.SaveAsXaml("c:\\data\\workflow"));
        }

        [Fact]
        public void SaveAsXamlIsOk()
        {
            const string sequence = "some xaml";
            var workflow = new Workflow("MyWorkflow", "My Workflow", sequence);
            var filePath = Path.Combine(Path.GetTempPath(), "MyWorkflow.xaml");
            workflow.SaveAsXaml(filePath);
            Assert.True(File.Exists(filePath));
        }
    }
}