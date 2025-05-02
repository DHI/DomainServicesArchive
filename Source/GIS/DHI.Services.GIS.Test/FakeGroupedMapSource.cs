namespace DHI.Services.GIS.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Maps;
    using Spatial;
    using SkiaSharp;

    public class FakeGroupedMapSource : BaseGroupedMapSource
    {
        private readonly Dictionary<string, Layer> _layers = new Dictionary<string, Layer>();

        public FakeGroupedMapSource(IEnumerable<Layer> layers)
        {
            foreach (var layer in layers)
            {
                _layers.Add(layer.Id, layer);
            }
        }

        public override IEnumerable<Layer> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            return _layers.Values.Where(layer => layer.Group == group);
        }

        public override bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            return _layers.Values.Any(layer => layer.Group == group);
        }

        public override IEnumerable<Layer> GetAll(ClaimsPrincipal user = null)
        {
            return _layers.Values;
        }

        public override Maybe<Layer> Get(string id, ClaimsPrincipal user = null)
        {
            return _layers.ContainsKey(id) ? _layers[id].ToMaybe() : Maybe.Empty<Layer>();
        }

        public override SKBitmap GetMap(MapStyle style, string crs, BoundingBox boundingBox, int width, int height, string filePath, DateTime? dateTime, string item, Parameters parameters)
        {
            throw new NotImplementedException();
        }

        public override SortedSet<DateTime> GetDateTimes(string id)
        {
            throw new NotImplementedException();
        }

        public override (Maybe<Stream>, string fileType, string fileName) GetStream(string id, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }
}