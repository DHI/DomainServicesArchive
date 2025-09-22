namespace DHI.Services.Samples.Radar.ViewModels.Types
{
    using System;
    public sealed class RepositoryTypeItem
    {
        public RepositoryTypeItem(Type type) { Type = type; }
        public Type Type { get; }
        public string DisplayName => Type.FullName;
        public override string ToString() => DisplayName;
    }

}
