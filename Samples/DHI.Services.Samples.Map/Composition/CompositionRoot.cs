using DHI.Services.GIS.Maps;
using DHI.Services.Provider.MIKECore;
using System.IO;

namespace DHI.Services.Samples.Map.Composition
{
    /// <summary>
    /// Wires MapService + optional MapStyleService (json) + Dfs2MapSource.
    /// </summary>
    public static class CompositionRoot
    {
        public static MapRuntime Wire(string dfs2PathOrFolder, string? stylesJsonPath = null)
        {
            // 1) Map source (MIKECore Dfs2)
            var source = new Dfs2MapSource(dfs2PathOrFolder);

            // 2) Map style service (optional if styles.json exists)
            MapStyleService? styleSvc = null;
            stylesJsonPath ??= Path.Combine(AppContext.BaseDirectory, "App_Data", "styles.json");
            if (File.Exists(stylesJsonPath))
            {
                var repo = new MapStyleRepository(stylesJsonPath);
                styleSvc = new MapStyleService(repo);
            }

            var mapSvc = new MapService(source, styleSvc);

            return new MapRuntime(mapSvc, source, dfs2PathOrFolder, stylesJsonPath, styleSvc);
        }
    }

    public sealed class MapRuntime
    {
        public MapRuntime(MapService service, IMapSource source, string root, string? stylesPath, MapStyleService? styleService)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            RootPath = root;
            StylesPath = stylesPath;
            StyleService = styleService;
        }

        public MapService Service { get; }
        public IMapSource Source { get; }
        public string RootPath { get; }
        public string? StylesPath { get; }
        public MapStyleService? StyleService { get; }

        public bool HasStyles => StyleService != null;
    }
}
