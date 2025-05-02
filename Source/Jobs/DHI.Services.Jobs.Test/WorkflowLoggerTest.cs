namespace DHI.Services.Jobs.Test;

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Workflows;
using Xunit;

public class WorkflowLoggerTest : IDisposable
{
    private readonly DirectoryInfo _tempDirectory;

    public WorkflowLoggerTest()
    {
        _tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "DsJobsTest"));
        if (_tempDirectory.Exists)
        {
            _tempDirectory.Delete(true);
        }

        _tempDirectory.Create();
    }

    [Fact]
    public void LogIsOk()
    {
        var logger = new WorkflowLogger(_tempDirectory.FullName, "ThisTest", "file");
        logger.Log(LogLevel.Information, "This is a {Message}", "template");

        var allLogs = File.ReadAllText(Path.Combine(_tempDirectory.FullName, "file.log"));
        Assert.Equal($"LogSource:ThisTest. LogLevel:Information. LogText:This is a template{Environment.NewLine}", allLogs[33..]);
    }

    [Fact]
    public void LogManyIsOk()
    {
        var logger = new WorkflowLogger(_tempDirectory.FullName, "ThisTest", "file");

        for (int i = 0; i < 10; i++)
        {
            logger.Log(LogLevel.Information, "{I}: this is a template", i);
            if (i % 4 == 0)
            {
                logger.Log(LogLevel.Warning, "{I}: WARNING!!!", i);
            }
        }

        var allLogs = File.ReadAllLines(Path.Combine(_tempDirectory.FullName, "file.log"));
        Assert.Equal(13, allLogs.Length);
    }

    public void Dispose()
    {
        _tempDirectory.Delete(true);
    }
}