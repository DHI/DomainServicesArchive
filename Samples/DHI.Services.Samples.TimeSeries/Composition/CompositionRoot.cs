using DHI.Services.Provider.MIKECore;
using DHI.Services.TimeSeries;
using System;
using System.IO;

namespace DHI.Services.Samples.TimeSeries.Composition
{
    /// <summary>Creates the TimeSeries service with the real repository.</summary>
    public static class CompositionRoot
    {
        public static UpdatableTimeSeriesService<string, double> CreateService(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A DFS0 file path is required.", nameof(filePath));

            var abs = ToAbsolutePath(filePath);
            var repo = new Dfs0TimeSeriesRepository(abs);
            return new UpdatableTimeSeriesService<string, double>(repo);
        }

        // ---------- helpers ----------
        private static string BaseDir => AppContext.BaseDirectory;
        private static string ToAbsolutePath(string path) =>
            Path.IsPathRooted(path) ? Path.GetFullPath(path)
                                    : Path.GetFullPath(Path.Combine(BaseDir, path));
    }
}
