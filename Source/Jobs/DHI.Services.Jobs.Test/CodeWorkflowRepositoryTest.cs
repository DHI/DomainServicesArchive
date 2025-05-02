namespace DHI.Services.Jobs.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Workflows;
    using Xunit;

    public class CodeWorkflowRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly CodeWorkflowRepository _repository;

        public CodeWorkflowRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "MyWorkflows.json");
            File.Copy(@"../../../Data/MyWorkflows.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            _repository = new CodeWorkflowRepository(_filePath);
        }

        [Fact]
        public void CanInitialize()
        {
            Assert.NotNull(_repository);
        }

        [Fact]
        public void ImplementsCodeWorkflowRepositoryInterface()
        {
            Assert.IsAssignableFrom<ICodeWorkflowRepository>(_repository);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new CodeWorkflowRepository(null));
        }

        [Fact]
        public void GetAllIsOk()
        {
            var workflows = _repository.GetAll().ToList();
            Assert.Single(workflows);
            Assert.Equal("MyWorkflows.CreateAndDeleteDirectory", workflows[0].Id);
        }

        [Fact]
        public void GetIsOk()
        {
            var workflow = _repository.Get("MyWorkflows.CreateAndDeleteDirectory").Value;
            Assert.Equal("MyWorkflows.CreateAndDeleteDirectory", workflow.Id);
            Assert.Equal(1, workflow.Parameters.Count);
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.Equal(1, _repository.Count());
        }

        [Fact]
        public void ContainsIsOk()
        {
            Assert.True(_repository.Contains("MyWorkflows.CreateAndDeleteDirectory"));
            Assert.False(_repository.Contains("NonExisting"));
        }

        [Fact]
        public void GetIdsIsOk()
        {
            var ids = _repository.GetIds().ToList();
            Assert.Single(ids);
            Assert.Equal("MyWorkflows.CreateAndDeleteDirectory", ids[0]);
        }

        [Fact]
        public void AddAndRemoveIsOk()
        {
            var codeWorkflow = new CodeWorkflow("removeMe", "Not worth it anyway", "MyWorkflows")
            {
                HostGroup = "MyHostGroup",
                Timeout = TimeSpan.FromMinutes(30)
            };
            _repository.Add(codeWorkflow);
            Assert.True(_repository.Contains(codeWorkflow.Id));
            var maybe = _repository.Get(codeWorkflow.Id);
            Assert.True(maybe.HasValue);
            var workflow = maybe.Value;
            Assert.Equal("MyHostGroup", workflow.HostGroup);
            Assert.Equal(TimeSpan.FromMinutes(30), workflow.Timeout);

            _repository.Remove(workflow.Id);
            Assert.False(_repository.Contains(workflow.Id));
        }

        [Fact]
        public void AddAndRemoveByPredicateIsOk()
        {
            var workflow = new CodeWorkflow("removeMe", "not worth it anyway", "MyWorkflows");
            _repository.Add(workflow);
            Assert.True(_repository.Contains(workflow.Id));

            _repository.Remove(w => w.Name.Contains("not worth it"));
            Assert.False(_repository.Contains(workflow.Id));
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }
    }
}