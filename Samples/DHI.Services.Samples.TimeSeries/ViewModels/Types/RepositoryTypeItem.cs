using System;
using System.IO;

namespace DHI.Services.Samples.TimeSeries.ViewModels.Types
{
    public sealed class RepositoryTypeItem
    {
        public RepositoryTypeItem(Type type) { Type = type; }
        public Type Type { get; }
        public string DisplayName => Type.FullName ?? Type.Name;
        public override string ToString() => DisplayName;
    }

    public sealed class Dfs0FileItem
    {
        public Dfs0FileItem(string fullPath) { FullPath = fullPath; }
        public string FullPath { get; }
        public string FileName => Path.GetFileName(FullPath);
        public string DisplayName => FileName;
        public override string ToString() => DisplayName;
    }

    public sealed class SeriesIdItem
    {
        public SeriesIdItem(string id) { Id = id; }
        public string Id { get; }
        public override string ToString() => Id;
    }
}
