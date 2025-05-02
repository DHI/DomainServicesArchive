namespace DHI.Services.Jobs.WebApi
{
    public class JobStatusUpdateDTO
    {
        public int? Progress { get; set; }

        public string StatusMessage { get; set; }

        public JobStatus JobStatus { get; set; }
    }
}
