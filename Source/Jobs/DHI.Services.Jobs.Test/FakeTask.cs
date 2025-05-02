namespace DHI.Services.Jobs.Test
{
    using DHI.Services.Jobs.Workflows;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class FakeTask<TId> : BaseGroupedEntity<TId>, ITask<TId>, IDynamicTimeoutTask
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public FakeTask(TId id, string name, string group)
            : base(id, name, group)
        {
        }

        public TimeSpan? Timeout { get; set; }

        public virtual IDictionary<string, object> Parameters => _parameters;

        public string HostGroup => null;

        public TimeSpan? WorkflowTimeout { get; set; }
        public TimeSpan? TerminationGracePeriod { get; set; }

        public bool ShouldSerializeParameters()
        {
            return _parameters.Count > 0;
        }
    }
}
