namespace DHI.Services.Jobs
{
    using System.Threading.Tasks;

    public interface ICloudInstanceHandler
    {
        Task Start();

        Task Stop();

        CloudInstanceStatus GetStatus();
    }
}