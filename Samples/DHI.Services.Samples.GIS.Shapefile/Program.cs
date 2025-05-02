namespace DHI.Services.Samples.GIS.Shapefile
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using DHI.Services.GIS;
    using Provider.ShapeFile;

    internal class Program
    {
        private static void Main()
        {
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var featureRepository = new FeatureRepository(Path.Combine(binFolder, @"..\..\"));
            var gisService = new GisService<string>(featureRepository);
            var query = new List<QueryCondition> { new QueryCondition("OmrId", QueryOperator.Equal, "1") };
            var featureCollection = gisService.Get("stationer.shp", query);
            var count = featureCollection.Features.Count(feature => ((string)feature.AttributeValues["Navn"]).Contains("Roskilde"));
            Console.WriteLine($"Number of features with a name containing 'Roskilde': {count}");
            Console.ReadLine();
        }
    }
}