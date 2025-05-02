namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;
    using DHI.Services.Jobs.Workflows.WF;
    using Jobs;
    using Workflows;
    using Xunit;

    public sealed class WorkflowXamlFilesRepositoryTests : IDisposable
    {
        private readonly string _filePath;
        private readonly string _directoryPath;
        private readonly WorkflowXamlFilesRepository _repository;

        public WorkflowXamlFilesRepositoryTests()
        {
            var tmpPath = Path.GetTempPath();
            _filePath = Path.Combine(tmpPath, "xaml.json");
            _directoryPath = Path.Combine(tmpPath, "xaml");
            File.Copy(@"../../../Data/Workflows/xaml.json", _filePath, true);
            DirectoryCopy(@"../../../Data/Workflows/xaml/", _directoryPath, true);
            new FileInfo(_filePath).IsReadOnly = false;
            var converters = new List<JsonConverter>
            {
                new WorkflowConverter()
            };
            _repository = new WorkflowXamlFilesRepository(_filePath, converters);
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
            Assert.Equal(3, all.Count);
            Assert.Equal("xamlTest", all[0].Id);
            Assert.Equal("xamlTest2", all[1].Id);
        }

        [Fact]
        public void GetIsOk()
        {
            var single = _repository.Get("xamlTest").Value;
            Assert.Equal("xamlTest", single.Id);
            Assert.Single(single.Parameters);
        }

        [Fact]
        public void CountIsOk()
        {
            Assert.Equal(3, _repository.Count());
        }

        [Fact]
        public void ContainsIsOk()
        {
            Assert.True(_repository.Contains("xamlTest"));
            Assert.False(_repository.Contains("someRandomName"));
        }

        [Fact]
        public void GetIdsIsOk()
        {
            var ids = _repository.GetIds().ToList();
            Assert.Equal(3, ids.Count);
            Assert.Equal("xamlTest", ids[0]);
            Assert.Equal("xamlTest2", ids[1]);
        }

        [Fact]
        public void AddAndRemoveIsOk()
        {
            _repository.Add(new Workflow("removeMe", "addedName", "Kevin, did you reinforce the take off ramp... No we didn't have time"));
            Assert.Equal(4, _repository.Count());

            _repository.Remove(x => x.Id == "removeMe");
            Assert.Equal(3, _repository.Count());
        }

        [Fact]
        public void UpdateIsOk()
        {
            var maybe = _repository.Get("xamlTest");
            Assert.True(maybe.HasValue);
            Assert.IsType<Workflow>(maybe.Value);
            var workflow = (Workflow)maybe.Value;
            workflow.Parameters.Add("foo", "bar");
            _repository.Update(workflow);
            Assert.Contains("foo", _repository.Get(workflow.Id).Value.Parameters);
        }

        [Fact]
        public void UpdateShorterDefinitionIsOk()
        {
            var maybe = _repository.Get("LongTest");
            var workflow = (Workflow)maybe.Value;
            var workflowDefinition = @"<Sequence mc:Ignorable=""sap sads"" sap:VirtualizedContainerService.HintSize=""811,714"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""
            xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities""
            xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
            xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities""
            xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger""
            xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation""
            xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib""
            xmlns:si=""clr-namespace:System.IO;assembly=mscorlib""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <sap:WorkflowViewStateService.ViewState>
                <scg:Dictionary x:TypeArguments=""x:String, x:Object"">
                <x:Boolean x:Key=""IsExpanded"">True</x:Boolean>
                </scg:Dictionary>
                </sap:WorkflowViewStateService.ViewState>
                <If Condition=""True"" sap:VirtualizedContainerService.HintSize=""789,590"">
                </If>
                </Sequence>";
            workflow.Definition = workflowDefinition;
            _repository.Update(workflow);
            maybe = _repository.Get("LongTest");
            workflow = (Workflow)maybe.Value;
            Assert.Equal(workflowDefinition, workflow.Definition);
        }

        public void Dispose()
        {
            File.Delete(_filePath);
            Directory.Delete(_directoryPath, true);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDirName);
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (var subDir in dirs)
                {
                    var tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, true);
                }
            }
        }
    }
}