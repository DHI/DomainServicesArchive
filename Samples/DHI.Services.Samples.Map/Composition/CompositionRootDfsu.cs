namespace DHI.Services.Samples.Map.Composition
{
    using DHI.Services.GIS.Maps;
    using DHI.Services.Provider.MIKECore;
    using System.IO;

    /// <summary>
    /// Wires MapService + optional MapStyleService (json) + DfsuMapSource.
    /// </summary>
    public static class CompositionRootDfsu
    {
        public static MapRuntime Wire(string dfsuPathOrFolder, string? stylesJsonPath = null)
        {
            // 1) Map source (MIKECore Dfsu)
            var source = new DfsuMapSource(dfsuPathOrFolder);

            // 2) Map style service (optional if styles.json exists)
            MapStyleService? styleSvc = null;
            stylesJsonPath ??= Path.Combine(AppContext.BaseDirectory, "App_Data", "styles.json");
            if (File.Exists(stylesJsonPath))
            {
                var repo = new MapStyleRepository(stylesJsonPath);
                styleSvc = new MapStyleService(repo);
            }

            var mapSvc = new MapService(source, styleSvc);

            return new MapRuntime(mapSvc, source, dfsuPathOrFolder, stylesJsonPath, styleSvc);
        }
    }
}
