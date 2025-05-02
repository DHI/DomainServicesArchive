namespace DHI.Services.Jobs.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Workflows;
    using Xunit;

    public class CodeWorkflowServiceTest : IDisposable
    {
        private readonly string _tempFolder;

        public CodeWorkflowServiceTest()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);
        }

        [Fact]
        public void ImportFromThrowsIfWorkflowExists()
        {
            var assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), @"../../../Data/MyWorkflows.dll"));
            var repositoryPath = Path.Combine(_tempFolder, assembly.GetName().Name + ".json");
            var service = new CodeWorkflowService(new CodeWorkflowRepository(repositoryPath));
            service.ImportFrom(assembly);

            var workflows = service.GetAll().ToArray();
            Assert.NotEmpty(workflows);
            Assert.Contains(workflows, workflow => workflow.Id == "MyWorkflows.CreateAndDeleteDirectory");
            var e = Assert.Throws<ArgumentException>(() => service.ImportFrom(assembly));
            Assert.Contains("An item with the same key has already been added.", e.Message);
        }

        [Fact]
        public void ImportFromIsOk()
        {
            var assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), @"../../../Data/MyWorkflows.dll"));
            var repositoryPath = Path.Combine(_tempFolder, assembly.GetName().Name + ".json");
            var service = new CodeWorkflowService(new CodeWorkflowRepository(repositoryPath));
            service.ImportFrom(assembly);

            var workflows = service.GetAll().ToArray();
            Assert.NotEmpty(workflows);
            Assert.Contains(workflows, workflow => workflow.Id == "MyWorkflows.CreateAndDeleteDirectory");

            service.TryGet("MyWorkflows.CreateAndDeleteDirectory", out var workflow );
            Assert.Equal("MyHostGroup", workflow.HostGroup);
            Assert.Equal(TimeSpan.FromMinutes(30), workflow.Timeout);
            Assert.NotEmpty(workflow.Parameters);
            Assert.Contains("FolderName", workflow.Parameters.Keys);
            Assert.Equal("System.String", workflow.Parameters["FolderName"]);
        }

        [Fact]
        public void ImportFromWithAllowReplaceIsOk()
        {
            var assembly = Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), @"../../../Data/MyWorkflows.dll"));
            var repositoryPath = Path.Combine(_tempFolder, assembly.GetName().Name + ".json");
            var service = new CodeWorkflowService(new CodeWorkflowRepository(repositoryPath));
            service.ImportFrom(assembly);

            var workflows = service.GetAll().ToArray();
            Assert.NotEmpty(workflows);
            Assert.Contains(workflows, workflow => workflow.Id == "MyWorkflows.CreateAndDeleteDirectory");

            // Allow replace
            service.ImportFrom(assembly, true);
            Assert.NotEmpty(workflows);
            Assert.Contains(workflows, workflow => workflow.Id == "MyWorkflows.CreateAndDeleteDirectory");
        }

        public void Dispose()
        {
            Directory.Delete(_tempFolder, true);
        }
    }
}
