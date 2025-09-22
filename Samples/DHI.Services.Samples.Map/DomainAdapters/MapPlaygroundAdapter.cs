using DHI.Services.Provider.MIKECore;
using DHI.Services.Samples.Map.Composition;
using DHI.Spatial;
using SkiaSharp;
using System.IO;

namespace DHI.Services.Samples.Map.DomainAdapters
{
    public sealed class MapPlaygroundAdapter
    {
        private readonly MapRuntime _rt;

        public MapPlaygroundAdapter(MapRuntime runtime)
        {
            _rt = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public bool IsSingleFile => File.Exists(_rt.RootPath);

        /// <summary>
        /// List available file ids (filenames) under the selected path.
        /// If a single file is selected, return that file name.
        /// </summary>
        public IEnumerable<string> ListSourceIds()
        {
            if (IsSingleFile)
                return new[] { Path.GetFileName(_rt.RootPath) };

            if (Directory.Exists(_rt.RootPath))
            {
                // Choose pattern based on the wired source type
                var pattern = _rt.Source is DfsuMapSource ? "*.dfsu" : "*.dfs2";
                return Directory.EnumerateFiles(_rt.RootPath, pattern, SearchOption.TopDirectoryOnly)
                                .Select(Path.GetFileName);
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// List style IDs if styles.json is present. Otherwise empty.
        /// </summary>
        public IEnumerable<string> ListStyleIds()
        {
            if (!_rt.HasStyles) return Array.Empty<string>();
            return _rt.StyleService!.GetAll().Select(s => s.Id);
        }

        /// <summary>
        /// Get supported time steps for a given file id (filename).
        /// </summary>
        public SortedSet<DateTime> GetDateTimes(string sourceId)
            => _rt.Service.GetDateTimes(sourceId);

        /// <summary>
        /// Convenience: read spatial extent from provider (DFS2/DFSU).
        /// </summary>
        public BoundingBox? TryGetLayerBounds(string sourceId)
        {
            if (_rt.Source is Dfs2MapSource dfs2)
            {
                var layer = dfs2.GetLayerInfo(sourceId);
                return layer.BoundingBox;
            }
            else if (_rt.Source is DfsuMapSource dfsu)
            {
                var layer = dfsu.GetLayerInfo(sourceId);
                return layer.BoundingBox;
            }

            return null;
        }

        /// <summary>
        /// Render a map bitmap via the domain service.
        /// </summary>
        public SKBitmap Render(string styleOrStyleId, string crs, BoundingBox bbox,
                               int width, int height, string sourceId,
                               DateTime? time, string item, Parameters parameters)
        {
            return _rt.Service.GetMap(styleOrStyleId, crs, bbox, width, height, sourceId, time, item, parameters);
        }
    }
}
