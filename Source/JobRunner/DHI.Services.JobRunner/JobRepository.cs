namespace DHI.Services.JobRunner
{
    using System;
    using Jobs;

    public class JobRepository : JobRepository<Guid, string>
    {
        public JobRepository(string filePath) : base(filePath)
        {
        }
    }
}