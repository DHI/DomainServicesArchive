namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Collections.Generic;
    using Spatial;
    using SkiaSharp;

    public class MapService : IMapService
    {
        private readonly IMapSource _mapSource;
        private readonly MapStyleService _mapStyleService;

        public MapService(IMapSource mapSource, MapStyleService mapStyleService = null)
        {
            _mapSource = mapSource ?? throw new ArgumentNullException(nameof(mapSource));
            _mapStyleService = mapStyleService;
        }

        /// <inheritdoc />
        public SKBitmap GetMap(string style, string crs, BoundingBox boundingBox, int width, int height, string sourceId, DateTime? dateTime, string item, Parameters parameters)
        {
            return _mapSource.GetMap(GetMapStyle(style), crs, boundingBox, width, height, sourceId, dateTime, item, parameters);
        }

        /// <inheritdoc />
        public SortedDictionary<DateTime, SKBitmap> GetMaps(string style, BoundingBox boundingBox, SKSizeI size, Dictionary<DateTime, string> timeSteps, string item, Parameters parameters)
        {
            return _mapSource.GetMaps(GetMapStyle(style), boundingBox, size, timeSteps, item, parameters);
        }

        /// <inheritdoc />
        public SortedSet<DateTime> GetDateTimes(string id)
        {
            return _mapSource.GetDateTimes(id);
        }

        /// <inheritdoc />
        public SortedSet<DateTime> GetDateTimes(string id, DateRange dateRange)
        {
            return _mapSource.GetDateTimes(id, dateRange);
        }

        public static Type[] GetMapSourceTypes(string path = null)
        {
            return Service.GetProviderTypes<IMapSource>(path);
        }

        public static Type[] GetMapStyleRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IMapStyleRepository>(path);
        }

        private MapStyle GetMapStyle(string style)
        {
            if (_mapStyleService is null)
            {
                return new MapStyle("InjectedStyle", "Injected Style") { StyleCode = style };
            }
            else
            {
                return _mapStyleService.TryGet(style, out var ms)
                    ? ms
                    : throw new KeyNotFoundException($"\"{style}\": no such map style exists");
            }
        }
    }
}