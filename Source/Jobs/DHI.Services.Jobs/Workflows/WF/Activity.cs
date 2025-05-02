namespace DHI.Services.Jobs.Workflows
{
    using System;

    [Obsolete("This type will eventually be removed.")]
    public class Activity : BaseNamedEntity<string>
    {
        public Activity(string id, string name, Type type)
            : base(id, name)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}