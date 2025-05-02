namespace DHI.Services.Jobs.WorkflowWorker.Test
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class FakeTask<TId> : BaseGroupedEntity<TId>, ITask<TId>
    {
        private readonly Dictionary<string, object> _parameters = new();

        public FakeTask(TId id, string name, string group)
            : base(id, name, group)
        {
        }

        public TimeSpan? Timeout { get; set; }

        public virtual IDictionary<string, object> Parameters => _parameters;

        public string HostGroup => null;

        public bool ShouldSerializeParameters()
        {
            return _parameters.Count > 0;
        }
    }
}
