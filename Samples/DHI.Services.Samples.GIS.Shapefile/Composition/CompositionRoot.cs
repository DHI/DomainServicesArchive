namespace DHI.Services.Samples.GIS.Shapefile.Composition
{
    using DHI.Services.GIS;
    using DHI.Services.Provider.ShapeFile;

    /// <summary>
    /// Wires a read-only GIS service (GisService) backed by the ShapeFile provider.
    /// </summary>
    public static class CompositionRoot
    {
        public static GisRuntime Wire(string pathOrShpFile)
        {
            // The repository decides if path is a single file or a folder.
            var repo = new FeatureRepository(pathOrShpFile);
            var svc = new GisService<string>(repo);
            return new GisRuntime(svc, pathOrShpFile);
        }
    }

    public sealed class GisRuntime
    {
        public GisRuntime(GisService<string> service, string rootPath)
        {
            Service = service;
            RootPath = rootPath;
        }

        public GisService<string> Service { get; }
        public string RootPath { get; }
    }
}
