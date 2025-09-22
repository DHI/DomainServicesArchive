namespace DHI.Services.Jobs.Automations.Test;

using Triggers;
using Xunit;

public class DirectoryAutomationRepositoryTest : IDisposable
{
    public DirectoryAutomationRepositoryTest()
    {
        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), "DomainServicesTest", Guid.NewGuid().ToString());
        _tempDirectory = Directory.CreateDirectory(tempDirectoryPath);

        Utils.CopyDirectory(@"..\..\..\..\DHI.Services.Jobs.Automations.Test\Data\AutomationsRepository\", _tempDirectory.FullName, true);

        _repository = new DirectoryAutomationRepository<string>(_tempDirectory.FullName);
    }

    private readonly DirectoryInfo _tempDirectory;
    private readonly DirectoryAutomationRepository<string> _repository;

    [Fact]
    public void GetIsOk()
    {
        Assert.True(_repository.Get("TestAutomation").HasValue);
        Assert.True(_repository.Get("Group1/TestAutomation1").HasValue);
        Assert.True(_repository.Get("Group1/Group2/TestAutomation2").HasValue);
    }

    [Fact]
    public void AddIsOk()
    {
        var automation = new Automation<string>("NewAutomation", "NewGroup", "taskid")
        {
            TaskParameters = new Parameters
            {
                { "TestParameter", "TestValue" }
            },
        };

        _repository.Add(automation);
        Assert.True(File.Exists(Path.Combine(_tempDirectory.FullName, "NewGroup", "NewAutomation.json")));
        Assert.True(_repository.Get("TestAutomation").HasValue);
    }

    [Fact]
    public void UpdateIsOk()
    {
        var automation = new Automation<string>("NewAutomation", "NewGroup", "taskid")
        {
            TaskParameters = new Parameters
            {
                { "TestParameter", "TestValue" }
            },
        };

        _repository.Update(automation);
        Assert.True(File.Exists(Path.Combine(_tempDirectory.FullName, "NewGroup", "NewAutomation.json")));
        Assert.True(_repository.Get("TestAutomation").HasValue);
    }

    [Fact]
    public void CountIsOk()
    {
        Assert.Equal(3, _repository.Count());
    }

    [Fact]
    public void GetAllIsOk()
    {
        var automations = _repository.GetAll().ToArray();
        Assert.Equal(3, automations.Length);
        Assert.Collection(automations, au => Assert.Equal("TestAutomation", au.Id),
            au => Assert.Equal("Group1/TestAutomation1", au.Id),
            au => Assert.Equal("Group1/Group2/TestAutomation2", au.Id));
    }

    [Fact]
    public void GetIdsIsOk()
    {
        var ids = _repository.GetIds().ToArray();
        Assert.Equal(3, ids.Length);
        Assert.Collection(ids, id => Assert.Equal("TestAutomation", id),
            id => Assert.Equal("Group1/TestAutomation1", id),
            id => Assert.Equal("Group1/Group2/TestAutomation2", id));
    }

    [Theory]
    [InlineData("TestAutomation")]
    [InlineData("Group1/TestAutomation1")]
    [InlineData("Group1/Group2/TestAutomation2")]
    public void RemoveIsOk(string id)
    {
        _repository.Remove(id);
        Assert.False(File.Exists(Path.Combine(_tempDirectory.FullName, $"{id}.json")));
        Assert.False(_repository.Get(id).HasValue);
        Assert.Equal(2, _repository.Count());
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("Group1", true)]
    [InlineData("Group1/Group2", true)]
    [InlineData("Group1\\Group2", true)]
    [InlineData("Group2", false)]
    public void ContainsGroupIsOk(string group, bool expected)
    {
        Assert.Equal(expected, _repository.ContainsGroup(group));
    }

    [Fact]
    public void GetByGroupIsOk()
    {
        var automations = _repository.GetByGroup("Group1").ToArray();
        Assert.Equal(2, automations.Length);
        Assert.Collection(automations, au => Assert.Equal("Group1/TestAutomation1", au.Id),
            au => Assert.Equal("Group1/Group2/TestAutomation2", au.Id));
    }

    [Fact]
    public void GetFullNamesByGroupIsOk()
    {
        var fullNames = _repository.GetFullNames("Group1").ToArray();
        Assert.Equal(2, fullNames.Length);
        Assert.Collection(fullNames, name => Assert.Equal("Group1/TestAutomation1", name),
            name => Assert.Equal("Group1/Group2/TestAutomation2", name));
    }

    [Fact]
    public void GetFullNamesIsOk()
    {
        var fullNames = _repository.GetFullNames().ToArray();
        Assert.Equal(3, fullNames.Length);
        Assert.Collection(fullNames, name => Assert.Equal("TestAutomation", name),
            name => Assert.Equal("Group1/TestAutomation1", name),
            name => Assert.Equal("Group1/Group2/TestAutomation2", name));
    }

    [Fact]
    public void VersionFile_IsCreatedAndUpdated()
    {
        var versionPath = Path.Combine(_tempDirectory.FullName, "version.txt");

        if (File.Exists(versionPath)) File.Delete(versionPath);
        Assert.False(File.Exists(versionPath));

        var automation = new Automation<string>("VersionedAuto", "VersionGroup", "task");
        _repository.Add(automation);

        Assert.True(File.Exists(versionPath));
        var ts1 = _repository.GetVersionTimestamp();

        Thread.Sleep(10);
        _repository.TouchVersion();
        var ts2 = _repository.GetVersionTimestamp();

        Assert.True(ts2 > ts1);
    }

    [Fact]
    public void AddUpdateRemove_TriggersVersionUpdate()
    {
        var versionPath = Path.Combine(_tempDirectory.FullName, "version.txt");
        if (File.Exists(versionPath)) File.Delete(versionPath);

        var automation = new Automation<string>("VerTrack", "VerGroup", "task");
        _repository.Add(automation);
        var ts1 = _repository.GetVersionTimestamp();

        Thread.Sleep(10);
        automation.Tag = "updated";
        _repository.Update(automation);
        var ts2 = _repository.GetVersionTimestamp();
        Assert.True(ts2 > ts1);

        Thread.Sleep(10);
        _repository.Remove("VerGroup/VerTrack");
        var ts3 = _repository.GetVersionTimestamp();
        Assert.True(ts3 > ts2);
    }


    public void Dispose()
    {
        _tempDirectory.Delete(true);
    }
}