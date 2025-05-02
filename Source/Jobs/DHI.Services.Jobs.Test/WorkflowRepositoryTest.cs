namespace DHI.Services.Jobs.Test
{
    using DHI.Services.Jobs.Workflows.WF;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Workflows;
    using Xunit;

    public class WorkflowRepositoryTest : IDisposable
    {
        private readonly string _filePath;
        private readonly WorkflowRepository _repository;

        public WorkflowRepositoryTest()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "workflows.json");
            File.Copy(@"../../../Data/workflows.json", _filePath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            var converters = new List<JsonConverter>
            {
                new WorkflowConverter()
            };
            _repository = new WorkflowRepository(_filePath, converters);
        }

        [Fact]
        public void CanInitialize()
        {
            Assert.NotNull(_repository);
        }

        [Fact]
        public void ImplementsWorkflowRepositoryInterface()
        {
            Assert.IsAssignableFrom<IWorkflowRepository>(_repository);
        }

        [Fact]
        public void ImplementsTaskRepositoryInterface()
        {
            Assert.IsAssignableFrom<ITaskRepository<Workflow, string>>(_repository);
        }

        [Fact]
        public void CreateWithNullFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkflowRepository(null));
        }

        [Fact]
        public void GetAllIsOk()
        {
            var all = _repository.GetAll().ToList();
            Assert.Equal(2, all.Count);
            Assert.Equal("Test", all[0].Id);
            Assert.Equal("Test2", all[1].Id);
        }

        [Fact]
        public void GetIsOk()
        {
            var single = _repository.Get("Test").Value;
            Assert.Equal("Test", single.Id);
            Assert.Equal(2, single.Parameters.Count);
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.Equal(2, _repository.Count());
        }

        [Fact]
        public void ContainsIsOk()
        {
            Assert.True(_repository.Contains("Test"));
            Assert.False(_repository.Contains("Practice"));
        }

        [Fact]
        public void GetIdsIsOk()
        {
            var ids = _repository.GetIds().ToList();
            Assert.Equal(2, ids.Count);
            Assert.Equal("Test", ids[0]);
            Assert.Equal("Test2", ids[1]);
        }

        [Fact]
        public void AddAndRemoveIsOk()
        {
            _repository.Add(new Workflow("removeMe", "addedName", "Kevin, did you reinforce the take off ramp... No we didn't have time"));
            Assert.Equal(3, _repository.Count());

            _repository.Remove(x => x.Id == "removeMe");
            Assert.Equal(2, _repository.Count());
        }

        [Fact]
        public void UpdateIsOk()
        {
            var maybe = _repository.Get("Test");
            Assert.True(maybe.HasValue);
            Assert.IsType<Workflow>(maybe.Value);
            var workflow = (Workflow)maybe.Value;
            workflow.Parameters.Add("foo", "bar");
            _repository.Update(workflow);
            Assert.Contains("foo", _repository.Get(workflow.Id).Value.Parameters);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }
    }
}