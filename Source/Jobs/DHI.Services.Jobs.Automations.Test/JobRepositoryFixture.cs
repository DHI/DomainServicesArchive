namespace DHI.Services.Jobs.Automations.Test;

using System;
using System.IO;

public class JobRepositoryFixture : IDisposable
{
    public JobRepositoryFixture()
    {
        var fileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "__jobs.json"));
        fileInfo.Directory!.Create();
        FilePath = fileInfo.FullName;

        File.Copy(@"../../../Data/jobs.json", FilePath, true);
        var jobRepository = new JobRepository(FilePath);
        var job = new Job(Guid.NewGuid(), "myCompletedTask")
        {
            Status = JobStatus.Completed
        };

        jobRepository.Add(job);

        job = new Job(Guid.NewGuid(), "myPendingTask")
        {
            Status = JobStatus.Pending
        };

        jobRepository.Add(job);
    }

    public string FilePath { get; }

    public void Dispose()
    {
        File.Delete(FilePath);
    }
}