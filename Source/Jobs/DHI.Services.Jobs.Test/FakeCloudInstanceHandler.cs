namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Serializable]
    internal class FakeCloudInstanceHandler : ICloudInstanceHandler
    {
        private static readonly Dictionary<string, CloudInstanceStatus> Status = new Dictionary<string, CloudInstanceStatus>();
        private readonly string _hostId;

        public FakeCloudInstanceHandler(Parameters parameters)
        {
            _hostId = parameters["HostId"];
            if (!Status.ContainsKey(_hostId))
            {
                Status.Add(_hostId, CloudInstanceStatus.Stopped);
            }
        }

        public async Task Start()
        {
            if (Status[_hostId] == CloudInstanceStatus.Stopped)
            {
                Status[_hostId] = CloudInstanceStatus.Starting;
                await Task.Delay(1000);
                Status[_hostId] = CloudInstanceStatus.Running;
            }
        }

        public async Task Stop()
        {
            if (Status[_hostId] == CloudInstanceStatus.Running)
            {
                Status[_hostId] = CloudInstanceStatus.Stopping;
                await Task.Delay(1000);
                Status[_hostId] = CloudInstanceStatus.Stopped;
            }
        }

        public CloudInstanceStatus GetStatus()
        {
            return Status[_hostId];
        }
    }
}