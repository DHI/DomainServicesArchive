namespace DHI.Services.Jobs.Automations.Test;

using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using DHI.Services.Jobs.Automations;
using DHI.Services.Jobs.Automations.TriggerParametersExport;
using DHI.Services.Jobs.Automations.Triggers;
using Xunit;

public class TriggerRepositoryTest
{
    private readonly TriggerRepository _repository;
    private readonly int _expectedTriggerExportCount;

    public TriggerRepositoryTest()
    {
        var catalog = new AggregateCatalog();

        // Add assembly by reference
        var baseTriggerCatalog = new AssemblyCatalog(typeof(BaseTrigger).Assembly);
        catalog.Catalogs.Add(baseTriggerCatalog);

        // Add assembly by loading a directory
        var testTriggerCatalog = new DirectoryCatalog(Environment.CurrentDirectory, $"{typeof(FakeTrigger).Assembly.GetName().Name}.dll");
        catalog.Catalogs.Add(testTriggerCatalog);

        _expectedTriggerExportCount = catalog.Parts.Count(p => p.ExportDefinitions.Any(e => e.Metadata["ExportTypeIdentity"].ToString() == typeof(ITriggerParameters).FullName));

        var container = new CompositionContainer(catalog);

        _repository = new TriggerRepository(container);
    }

    [Fact]
    public void Get_ScheduledTrigger_ReturnsExpected()
    {
        var attributes = GetTriggerAttributes(typeof(ScheduledTriggerParameters));
        var expectedRequiredCount = attributes.Count(a => a.Required);

        var actual = _repository.Get(nameof(ScheduledTrigger)).Value;

        Assert.Equal(nameof(ScheduledTrigger), actual.Id);
        Assert.Equal(attributes.Length, actual.Properties.Count);
        Assert.Equal(expectedRequiredCount, actual.Required.Length);
    }

    [Fact]
    public void Get_NotValidId_ReturnsEmptyMaybe()
    {
        var parameters = _repository.Get("NotValid");

        Assert.False(parameters.HasValue);
    }

    [Fact]
    public void Count_ReturnsExpected()
    {
        var count = _repository.Count();

        Assert.Equal(_expectedTriggerExportCount, count);
    }

    [Fact]
    public void Contains_ReturnsExpected()
    {
        Assert.True(_repository.Contains(nameof(FalseTrigger)));
        Assert.False(_repository.Contains("NotValid"));
    }

    [Fact]
    public void GetAll_ReturnExpectedCount()
    {
        var all = _repository.GetAll().ToArray();

        Assert.Equal(_expectedTriggerExportCount, all.Length);
    }

    [Fact]
    public void GetIds_Includes_ReturnsExpected()
    {
        var allIds = _repository.GetIds().ToArray();

        Assert.Contains(nameof(FalseTrigger), allIds);
        Assert.Contains(nameof(ScheduledTrigger), allIds);
    }

    [Fact]
    public void GetIds_NotValidId_ReturnsExpected()
    {
        var allIds = _repository.GetIds().ToArray();

        Assert.DoesNotContain("NotValid", allIds);
    }

    private static TriggerParameterAttribute[] GetTriggerAttributes(Type type)
    {
        return type.GetProperties().SelectMany(Attribute.GetCustomAttributes)
                   .Where(a => a.GetType() == typeof(TriggerParameterAttribute))
                   .Select(a => a as TriggerParameterAttribute)
                   .ToArray();
    }
}