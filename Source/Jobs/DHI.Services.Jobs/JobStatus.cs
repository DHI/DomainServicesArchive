namespace DHI.Services.Jobs
{
    public enum JobStatus
    {
        Pending,
        Starting,
        InProgress,
        Completed,
        Error,
        Unknown,
        Cancel,
        Cancelling,
        Cancelled,
        TimingOut, //status applied to a job that has reached its timeout
        TimedOut //status applied to a job that has been terminated due to timeout
    }
}