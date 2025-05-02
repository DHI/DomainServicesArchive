namespace DHI.Services.Jobs
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Text.Json.Serialization;

    [Serializable]
    public class Host : BaseGroupedEntity<string>
    {
        private ICloudInstanceHandler _cloudInstanceHandler;

        public Host(string id, string name)
            : this(id, name, null)
        {
        }

        [JsonConstructor]
        public Host(string id, string name, string group)
            : base(id, name, group)
        {
            CloudInstanceParameters = new Parameters();
        }

        public string CloudInstanceHandlerType { get; set; }

        //need private set for System.Text.Json, do not use set
        [JsonInclude]
        public Parameters CloudInstanceParameters { get; private set; }

        public int RunningJobsLimit { get; set; } = 10;

        public int Priority { get; set; } = 1;

        [JsonIgnore]
        public ICloudInstanceHandler CloudInstanceHandler
        {
            get
            {
                if (!(_cloudInstanceHandler is null))
                {
                    return _cloudInstanceHandler;
                }

                if (!string.IsNullOrEmpty(CloudInstanceHandlerType))
                {
                    _cloudInstanceHandler = _CreateCloudInstanceHandler();
                }

                return _cloudInstanceHandler;
            }
        }

        public bool ShouldSerializeCloudInstanceParameters()
        {
            return CloudInstanceParameters.Any();
        }

        public bool IsCloudInstance()
        {
            return !(CloudInstanceHandler is null);
        }

        private ICloudInstanceHandler _CreateCloudInstanceHandler()
        {
            try
            {
                var cloudInstanceHandlerType = Type.GetType(CloudInstanceHandlerType, true);
                return (ICloudInstanceHandler)Activator.CreateInstance(cloudInstanceHandlerType, CloudInstanceParameters);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}